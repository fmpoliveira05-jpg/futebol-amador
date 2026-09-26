package com.example.amfootball.competicao

import androidx.compose.foundation.horizontalScroll
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.material3.AlertDialog
import androidx.compose.material3.Card
import androidx.compose.material3.HorizontalDivider
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedButton
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.Dp
import androidx.compose.ui.unit.dp
import androidx.hilt.navigation.compose.hiltViewModel
import androidx.lifecycle.SavedStateHandle
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import com.example.amfootball.data.local.SessionManager
import com.example.amfootball.ui.components.inputFields.SelectBox
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import javax.inject.Inject

/** Dados desportivos editáveis pelo próprio jogador. */
data class DadosDesportivos(
    val peso: String = "",
    val pe: Int? = null,
    val situacao: Int = 0,
    val nacionalidade: String = "",
    val paisNascimento: String = ""
)

@HiltViewModel
class PerfilJogadorViewModel @Inject constructor(
    private val servico: CompeticaoService,
    private val sessao: SessionManager,
    savedStateHandle: SavedStateHandle
) : CompeticaoViewModel() {

    private val meuId = sessao.fetchUserId()
    val jogadorId: String = savedStateHandle.get<String>("playerId")?.takeIf { it.isNotBlank() } ?: meuId
    val souEu: Boolean = jogadorId.isNotBlank() && jogadorId == meuId

    private val _perfil = MutableStateFlow<PerfilJogadorDto?>(null)
    val perfil: StateFlow<PerfilJogadorDto?> = _perfil.asStateFlow()

    init {
        recarregar()
    }

    fun recarregar() {
        if (jogadorId.isBlank()) return
        carregar { _perfil.value = servico.perfil(jogadorId) }
    }

    fun dadosAtuais(): DadosDesportivos {
        val p = _perfil.value ?: return DadosDesportivos()
        return DadosDesportivos(
            peso = p.weight?.toString() ?: "",
            pe = p.preferredFoot,
            situacao = p.status,
            nacionalidade = p.nationality ?: "",
            paisNascimento = p.countryOfBirth ?: ""
        )
    }

    /** Valida e grava; devolve a mensagem de erro de validação (ou nulo). */
    fun guardar(d: DadosDesportivos): String? {
        val peso = d.peso.trim().takeIf { it.isNotEmpty() }?.toIntOrNull()
        if (d.peso.isNotBlank() && (peso == null || peso < 40 || peso > 150)) {
            return "O peso tem de estar entre 40 e 150 kg."
        }
        if (d.nacionalidade.length > 56 || d.paisNascimento.length > 56) {
            return "O país tem no máximo 56 caracteres."
        }
        val p = _perfil.value ?: return "O perfil ainda não carregou."
        val s = sessao.getUserProfile() ?: return "Inicia sessão outra vez para editar o perfil."
        val dto = AtualizarPerfilDto(
            name = p.name,
            dateOfBirth = p.dateOfBirth?.take(10),
            address = s.address,
            email = s.email ?: s.loginResponseDto?.email,
            phone = s.phone ?: s.phoneNumber,
            position = p.position,
            height = p.height,
            weight = peso,
            preferredFoot = d.pe,
            status = d.situacao,
            nationality = d.nacionalidade.trim().ifBlank { null },
            countryOfBirth = d.paisNascimento.trim().ifBlank { null }
        )
        acao("Perfil atualizado.") {
            servico.atualizarPerfil(jogadorId, dto)
            _perfil.value = servico.perfil(jogadorId)
        }
        return null
    }
}

/**
 * Ficha desportiva do jogador, ao estilo do ZeroZero: dados pessoais, totais, percurso por época
 * e transferências. Aparece por baixo dos dados do perfil.
 */
@Composable
fun FichaDesportivaJogador(viewModel: PerfilJogadorViewModel = hiltViewModel()) {
    val perfil by viewModel.perfil.collectAsStateWithLifecycle()
    val aCarregar by viewModel.aCarregar.collectAsStateWithLifecycle()
    val erro by viewModel.erroCarga.collectAsStateWithLifecycle()
    val aviso by viewModel.aviso.collectAsStateWithLifecycle()
    var editar by remember { mutableStateOf(false) }

    if (editar) {
        DialogoDadosDesportivos(
            inicial = viewModel.dadosAtuais(),
            aoGuardar = { d -> viewModel.guardar(d).also { if (it == null) editar = false } },
            aoFechar = { editar = false }
        )
    }

    Column(modifier = Modifier.fillMaxWidth()) {
        TituloSeccao("Ficha desportiva")
        when {
            aCarregar && perfil == null -> Text("A carregar estatísticas…")
            erro != null && perfil == null -> Row(verticalAlignment = Alignment.CenterVertically) {
                Text(erro ?: "", color = MaterialTheme.colorScheme.error, modifier = Modifier.weight(1f))
                TextButton(onClick = viewModel::recarregar) { Text("Tentar de novo") }
            }
        }
        AvisoCompeticao(aviso?.texto, aviso?.erro == true)
        val p = perfil ?: return@Column

        CartaoCompeticao {
            LinhaFicha("Situação", Textos.situacao(p.status))
            LinhaFicha("Clube atual", p.currentTeam?.name ?: "Sem clube (jogador livre)")
            LinhaFicha("Na equipa desde", p.joinedTeamAt?.let { Datas.data(it) })
            if (p.isListed) LinhaFicha("Mercado", "Disponível para transferência")
            LinhaFicha("Data de nascimento", p.dateOfBirth?.let { "${Datas.data(it)} (${p.age} anos)" })
            LinhaFicha("Nacionalidade", p.nationality)
            LinhaFicha("País de nascimento", p.countryOfBirth)
            LinhaFicha("Posição", Textos.posicao(p.position))
            LinhaFicha("Pé preferido", Textos.pe(p.preferredFoot))
            LinhaFicha("Altura", if (p.height > 0) "${p.height} cm" else null)
            LinhaFicha("Peso", p.weight?.let { "$it kg" })
            if (viewModel.souEu) {
                OutlinedButton(onClick = { editar = true }, modifier = Modifier.padding(top = 8.dp)) {
                    Text("Editar dados desportivos")
                }
            }
        }

        val t = p.totals ?: StatsLineDto()
        TituloSeccao("Totais")
        Row(horizontalArrangement = Arrangement.spacedBy(8.dp), modifier = Modifier.fillMaxWidth()) {
            Numero("Jogos", t.games, Modifier.weight(1f))
            Numero("Golos", t.goals, Modifier.weight(1f))
            Numero("Assist.", t.assists, Modifier.weight(1f))
        }
        Row(horizontalArrangement = Arrangement.spacedBy(8.dp), modifier = Modifier.fillMaxWidth().padding(top = 8.dp)) {
            Numero("Minutos", t.minutes, Modifier.weight(1f))
            Numero("Amarelos", t.yellowCards, Modifier.weight(1f))
            Numero("Vermelhos", t.redCards, Modifier.weight(1f))
        }

        TituloSeccao("Percurso por época")
        val carreira = p.career.orEmpty()
        if (carreira.isEmpty()) {
            Text("Ainda sem jogos registados.")
        } else {
            Column(modifier = Modifier.horizontalScroll(rememberScrollState())) {
                LinhaCarreira("Época", "Equipa", "J", "G", "A", "Min", "AM", "VM", cabecalho = true)
                HorizontalDivider()
                carreira.forEach { c ->
                    LinhaCarreira(
                        c.season ?: "—", c.teamName ?: "—", c.games.toString(), c.goals.toString(),
                        c.assists.toString(), c.minutes.toString(), c.yellowCards.toString(), c.redCards.toString()
                    )
                }
            }
        }

        TituloSeccao("Transferências")
        val transferencias = p.transfers.orEmpty()
        if (transferencias.isEmpty()) Text("Sem movimentos registados.")
        transferencias.forEach { tr ->
            CartaoCompeticao {
                Text(Textos.tipoTransferencia(tr.kind), fontWeight = FontWeight.Bold)
                Text("${tr.fromTeamName ?: "Livre"} → ${tr.toTeamName ?: "Livre"}")
                Text(Datas.data(tr.date), style = MaterialTheme.typography.bodySmall)
            }
        }
    }
}

@Composable
private fun Numero(rotulo: String, valor: Int, modifier: Modifier) {
    Card(modifier = modifier) {
        Column(modifier = Modifier
            .fillMaxWidth()
            .padding(8.dp), horizontalAlignment = Alignment.CenterHorizontally) {
            Text(valor.toString(), style = MaterialTheme.typography.titleLarge, fontWeight = FontWeight.Bold)
            Text(rotulo, style = MaterialTheme.typography.labelSmall)
        }
    }
}

@Composable
private fun LinhaCarreira(vararg celulas: String, cabecalho: Boolean = false) {
    val larguras: List<Dp> = listOf(64.dp, 120.dp, 32.dp, 32.dp, 32.dp, 48.dp, 36.dp, 36.dp)
    Row(modifier = Modifier.padding(vertical = 6.dp)) {
        celulas.forEachIndexed { i, texto ->
            Text(
                texto,
                modifier = Modifier.width(larguras.getOrElse(i) { 40.dp }),
                textAlign = if (i == 1) TextAlign.Start else TextAlign.Center,
                fontWeight = if (cabecalho) FontWeight.Bold else FontWeight.Normal,
                style = MaterialTheme.typography.bodySmall,
                maxLines = 1
            )
        }
    }
}

@Composable
private fun DialogoDadosDesportivos(
    inicial: DadosDesportivos,
    aoGuardar: (DadosDesportivos) -> String?,
    aoFechar: () -> Unit
) {
    var d by remember { mutableStateOf(inicial) }
    var erro by remember { mutableStateOf<String?>(null) }
    AlertDialog(
        onDismissRequest = aoFechar,
        title = { Text("Dados desportivos") },
        text = {
            Column(verticalArrangement = Arrangement.spacedBy(8.dp)) {
                OutlinedTextField(
                    value = d.peso,
                    onValueChange = { v -> d = d.copy(peso = v.filter { it.isDigit() }.take(3)) },
                    label = { Text("Peso (kg)") },
                    keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number),
                    singleLine = true
                )
                SelectBox(
                    list = listOf<Int?>(null, 0, 1, 2),
                    selectedValue = d.pe,
                    onSelectItem = { d = d.copy(pe = it) },
                    itemToString = { if (it == null) "Pé preferido por indicar" else "Pé ${Textos.pe(it).lowercase()}" }
                )
                SelectBox(
                    list = listOf(0, 1, 2),
                    selectedValue = d.situacao,
                    onSelectItem = { d = d.copy(situacao = it) },
                    itemToString = { Textos.situacao(it) }
                )
                OutlinedTextField(
                    value = d.nacionalidade,
                    onValueChange = { d = d.copy(nacionalidade = it.take(56)) },
                    label = { Text("Nacionalidade") },
                    singleLine = true
                )
                OutlinedTextField(
                    value = d.paisNascimento,
                    onValueChange = { d = d.copy(paisNascimento = it.take(56)) },
                    label = { Text("País de nascimento") },
                    singleLine = true
                )
                erro?.let { Text(it, color = MaterialTheme.colorScheme.error) }
            }
        },
        confirmButton = { TextButton(onClick = { erro = aoGuardar(d) }) { Text("Guardar") } },
        dismissButton = { TextButton(onClick = aoFechar) { Text("Cancelar") } }
    )
}
