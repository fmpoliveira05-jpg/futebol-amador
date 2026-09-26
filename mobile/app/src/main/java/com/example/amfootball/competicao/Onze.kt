package com.example.amfootball.competicao

import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.Button
import androidx.compose.material3.Checkbox
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedButton
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.hilt.navigation.compose.hiltViewModel
import androidx.lifecycle.SavedStateHandle
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import com.example.amfootball.data.filters.FilterMembersTeam
import com.example.amfootball.data.local.SessionManager
import com.example.amfootball.data.remote.services.TeamService
import com.example.amfootball.ui.components.LoadingPage
import com.example.amfootball.ui.components.inputFields.SelectBox
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import javax.inject.Inject

/** Estado do onze a ser montado no ecrã. */
data class EstadoOnze(
    val tatica: String = "",
    /** Posição (slot) → id do jogador. */
    val titulares: Map<Int, String> = emptyMap(),
    val banco: List<String> = emptyList()
)

@HiltViewModel
class OnzeViewModel @Inject constructor(
    private val servico: CompeticaoService,
    private val equipas: TeamService,
    sessao: SessionManager,
    savedStateHandle: SavedStateHandle
) : CompeticaoViewModel() {

    val jogoId: String = savedStateHandle.get<String>(RotasCompeticao.ARG_JOGO) ?: ""
    private val perfil = sessao.getUserProfile()
    private val equipaId: String = perfil?.effectiveTeamId ?: ""
    val souAdmin: Boolean = perfil?.isAdmin == true

    private val _taticas = MutableStateFlow<List<FormationDto>>(emptyList())
    val taticas: StateFlow<List<FormationDto>> = _taticas.asStateFlow()

    private val _plantel = MutableStateFlow<List<JogadorPlantel>>(emptyList())
    val plantel: StateFlow<List<JogadorPlantel>> = _plantel.asStateFlow()

    private val _onze = MutableStateFlow<LineupDto?>(null)
    val onze: StateFlow<LineupDto?> = _onze.asStateFlow()

    private val _estado = MutableStateFlow(EstadoOnze())
    val estado: StateFlow<EstadoOnze> = _estado.asStateFlow()

    init {
        recarregar()
    }

    fun recarregar() {
        carregar {
            _taticas.value = servico.taticas()
            _plantel.value = equipas.getListMembers(equipaId, FilterMembersTeam())
                .map { JogadorPlantel(it.id, it.name, it.positionId) }
            val o = servico.onze(jogoId, equipaId)
            _onze.value = o
            val tatica = o.formation.takeIf { f -> _taticas.value.any { it.code == f } }
                ?: _taticas.value.firstOrNull { it.code == "4-3-3" }?.code
                ?: _taticas.value.firstOrNull()?.code ?: ""
            _estado.value = EstadoOnze(
                tatica = tatica,
                titulares = o.starters.orEmpty().mapNotNull { s -> s.playerId?.let { s.slot to it } }.toMap(),
                banco = o.bench.orEmpty().map { it.playerId }
            )
        }
    }

    fun taticaAtual(): FormationDto? = _taticas.value.firstOrNull { it.code == _estado.value.tatica }

    fun mudarTatica(codigo: String) {
        val nova = _taticas.value.firstOrNull { it.code == codigo } ?: return
        val slots = nova.slots.orEmpty().map { it.slot }.toSet()
        _estado.value = _estado.value.copy(
            tatica = codigo,
            titulares = _estado.value.titulares.filterKeys { it in slots }
        )
    }

    fun escolher(slot: Int, jogadorId: String?) {
        val e = _estado.value
        val titulares = e.titulares.filterValues { it != jogadorId }.toMutableMap()
        if (jogadorId == null) titulares.remove(slot) else titulares[slot] = jogadorId
        _estado.value = e.copy(titulares = titulares, banco = e.banco - setOfNotNull(jogadorId))
    }

    fun alternarBanco(jogadorId: String) {
        val e = _estado.value
        _estado.value = when {
            jogadorId in e.banco -> e.copy(banco = e.banco - jogadorId)
            e.banco.size >= MAX_BANCO -> { avisar("O banco tem no máximo $MAX_BANCO suplentes.", true); e }
            else -> e.copy(banco = e.banco + jogadorId)
        }
    }

    /** Preenche as posições vazias com jogadores da posição certa (ou, se faltarem, com outros). */
    fun preencher() {
        val tatica = taticaAtual() ?: return
        val titulares = _estado.value.titulares.toMutableMap()
        tatica.slots.orEmpty().sortedBy { it.slot }.forEach { s ->
            if (titulares[s.slot] == null) {
                val candidato = RegrasOnze.candidatos(_plantel.value, s.role, titulares.values.toSet(), todos = true)
                    .firstOrNull()
                if (candidato != null) titulares[s.slot] = candidato.id
            }
        }
        val usados = titulares.values.toSet()
        _estado.value = _estado.value.copy(
            titulares = titulares,
            banco = _estado.value.banco.filter { it !in usados }
        )
    }

    fun guardar() {
        val tatica = taticaAtual() ?: return avisar("Escolhe uma tática.", true)
        val e = _estado.value
        val faltam = tatica.slots.orEmpty().count { e.titulares[it.slot] == null }
        if (faltam > 0) {
            return avisar("Faltam $faltam posições. Podes usar “Preencher o resto”.", true)
        }
        val dto = SaveLineupDto(
            formation = e.tatica,
            starters = e.titulares.map { (slot, id) -> SaveStarterDto(slot, id) },
            bench = e.banco
        )
        acao("Onze guardado.") { _onze.value = servico.guardarOnze(jogoId, equipaId, dto) }
    }

    fun nome(id: String?): String? = _plantel.value.firstOrNull { it.id == id }?.nome

    companion object {
        const val MAX_BANCO = 12
    }
}

@Composable
fun OnzeScreen(viewModel: OnzeViewModel = hiltViewModel()) {
    val aCarregar by viewModel.aCarregar.collectAsStateWithLifecycle()
    val erro by viewModel.erroCarga.collectAsStateWithLifecycle()
    val taticas by viewModel.taticas.collectAsStateWithLifecycle()
    val plantel by viewModel.plantel.collectAsStateWithLifecycle()
    val onze by viewModel.onze.collectAsStateWithLifecycle()
    val estado by viewModel.estado.collectAsStateWithLifecycle()
    val aviso by viewModel.aviso.collectAsStateWithLifecycle()
    val aGuardar by viewModel.aGuardar.collectAsStateWithLifecycle()
    var slotAtivo by remember { mutableStateOf<Int?>(null) }
    var todasPosicoes by remember { mutableStateOf(false) }

    LoadingPage(isLoading = aCarregar, errorMsg = erro, retry = viewModel::recarregar, content = {
        val o = onze
        val editavel = viewModel.souAdmin && o != null && !o.isLocked
        val tatica = taticas.firstOrNull { it.code == estado.tatica }
        Column(modifier = Modifier
            .fillMaxSize()
            .verticalScroll(rememberScrollState())
            .padding(16.dp)) {
            Text("Onze inicial", style = MaterialTheme.typography.headlineSmall, fontWeight = FontWeight.Bold)
            if (o != null) {
                Text(
                    when {
                        o.isLocked -> "O prazo terminou: o onze já não pode ser alterado."
                        else -> "Podes alterar até ${Datas.dataHora(o.deadline)}. Se não estiver completo, é preenchido automaticamente."
                    },
                    style = MaterialTheme.typography.bodySmall,
                    color = MaterialTheme.colorScheme.onSurfaceVariant
                )
                if (o.isAutoFilled) Text("Este onze foi preenchido automaticamente.", style = MaterialTheme.typography.bodySmall)
            }
            AvisoCompeticao(aviso?.texto, aviso?.erro == true)

            if (taticas.isNotEmpty()) {
                SelectBox(
                    list = taticas.map { it.code },
                    selectedValue = estado.tatica,
                    onSelectItem = { if (editavel) viewModel.mudarTatica(it) },
                    itemToString = { "Tática $it" },
                    modifier = Modifier
                        .fillMaxWidth()
                        .padding(vertical = 8.dp)
                )
            }

            val pontos = tatica?.slots.orEmpty().map { s ->
                PontoCampo(s.slot, s.x, s.y, s.positionCode, viewModel.nome(estado.titulares[s.slot]))
            }
            val aoTocar: ((Int) -> Unit)? = if (editavel) {
                { slot -> slotAtivo = if (slotAtivo == slot) null else slot }
            } else {
                null
            }
            CampoFutebol(pontos = pontos, aoTocar = aoTocar, selecionado = slotAtivo)

            val ativo = tatica?.slots?.firstOrNull { it.slot == slotAtivo }
            if (editavel && ativo != null) {
                TituloSeccao("Posição ${ativo.positionCode} (${Textos.posicao(ativo.role)})")
                Row(verticalAlignment = Alignment.CenterVertically) {
                    Checkbox(checked = todasPosicoes, onCheckedChange = { todasPosicoes = it })
                    Text("Mostrar jogadores de outras posições")
                }
                val escolhidos = estado.titulares.filterKeys { it != ativo.slot }.values.toSet()
                val candidatos = RegrasOnze.candidatos(plantel, ativo.role, escolhidos, todasPosicoes)
                if (candidatos.isEmpty()) Text("Não há jogadores livres para esta posição.")
                candidatos.forEach { j ->
                    Text(
                        text = "${j.nome} · ${Textos.posicaoCurta(j.posicao)}" +
                            if (estado.titulares[ativo.slot] == j.id) " (escolhido)" else "",
                        modifier = Modifier
                            .fillMaxWidth()
                            .clickable { viewModel.escolher(ativo.slot, j.id); slotAtivo = null }
                            .padding(vertical = 10.dp)
                    )
                }
                if (estado.titulares[ativo.slot] != null) {
                    OutlinedButton(onClick = { viewModel.escolher(ativo.slot, null) }) { Text("Deixar vazia") }
                }
            } else if (editavel) {
                Text("Toca numa posição do campo para escolher o jogador.", style = MaterialTheme.typography.bodySmall,
                    modifier = Modifier.padding(top = 8.dp))
            }

            TituloSeccao("Banco (${estado.banco.size}/${OnzeViewModel.MAX_BANCO})")
            val titulares = estado.titulares.values.toSet()
            val disponiveis = plantel.filter { it.id !in titulares }.sortedBy { it.nome.lowercase() }
            if (disponiveis.isEmpty()) Text("Não sobram jogadores para o banco.")
            disponiveis.forEach { j ->
                Row(verticalAlignment = Alignment.CenterVertically) {
                    Checkbox(
                        checked = j.id in estado.banco,
                        onCheckedChange = { viewModel.alternarBanco(j.id) },
                        enabled = editavel
                    )
                    Text("${j.nome} · ${Textos.posicaoCurta(j.posicao)}")
                }
            }

            if (editavel) {
                Row(horizontalArrangement = Arrangement.spacedBy(8.dp), modifier = Modifier.padding(top = 16.dp)) {
                    OutlinedButton(onClick = viewModel::preencher, enabled = !aGuardar) { Text("Preencher o resto") }
                    Button(onClick = viewModel::guardar, enabled = !aGuardar) { Text("Guardar onze") }
                }
            }
        }
    })
}
