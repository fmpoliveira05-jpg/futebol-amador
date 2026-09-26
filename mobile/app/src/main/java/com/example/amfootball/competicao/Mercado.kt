package com.example.amfootball.competicao

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.material3.AlertDialog
import androidx.compose.material3.Button
import androidx.compose.material3.Checkbox
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
import androidx.compose.ui.unit.dp
import androidx.hilt.navigation.compose.hiltViewModel
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import androidx.navigation.NavHostController
import com.example.amfootball.data.local.SessionManager
import com.example.amfootball.ui.components.LoadingPage
import com.example.amfootball.ui.components.inputFields.SelectBox
import com.example.amfootball.ui.navigation.objects.Routes
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import javax.inject.Inject

@HiltViewModel
class MercadoViewModel @Inject constructor(
    private val servico: CompeticaoService,
    sessao: SessionManager
) : CompeticaoViewModel() {

    private val equipaId: String = sessao.getUserProfile()?.effectiveTeamId ?: ""

    private val _filtros = MutableStateFlow(FiltrosMercado())
    val filtros: StateFlow<FiltrosMercado> = _filtros.asStateFlow()

    private val _jogadores = MutableStateFlow<List<MarketPlayerDto>>(emptyList())
    val jogadores: StateFlow<List<MarketPlayerDto>> = _jogadores.asStateFlow()

    private val _ligas = MutableStateFlow<List<LeagueDto>>(emptyList())
    val ligas: StateFlow<List<LeagueDto>> = _ligas.asStateFlow()

    /** Jogadores a quem esta equipa já fez proposta nesta sessão. */
    private val _propostos = MutableStateFlow<Set<String>>(emptySet())
    val propostos: StateFlow<Set<String>> = _propostos.asStateFlow()

    init {
        carregar {
            _ligas.value = try { servico.ligas().sortedBy { it.level } } catch (e: Exception) { emptyList() }
            _jogadores.value = servico.mercado(equipaId, _filtros.value)
        }
    }

    fun mudarFiltros(novos: FiltrosMercado) {
        _filtros.value = novos
    }

    fun pesquisar() {
        acao(null) { _jogadores.value = servico.mercado(equipaId, _filtros.value) }
    }

    fun limpar() {
        _filtros.value = FiltrosMercado()
        pesquisar()
    }

    fun proporTransferencia(jogador: MarketPlayerDto, mensagem: String) {
        val texto = if (jogador.teamId == null || jogador.isListed) {
            "Proposta enviada a ${jogador.name}: falta só a resposta do jogador."
        } else {
            "Proposta enviada: falta o acordo do clube (${jogador.teamName}) e do jogador."
        }
        acao(texto) {
            servico.fazerProposta(equipaId, jogador.playerId, mensagem.trim().ifBlank { null })
            _propostos.value = _propostos.value + jogador.playerId
        }
    }
}

@Composable
fun MercadoScreen(
    navHostController: NavHostController,
    viewModel: MercadoViewModel = hiltViewModel()
) {
    val aCarregar by viewModel.aCarregar.collectAsStateWithLifecycle()
    val erro by viewModel.erroCarga.collectAsStateWithLifecycle()
    val filtros by viewModel.filtros.collectAsStateWithLifecycle()
    val jogadores by viewModel.jogadores.collectAsStateWithLifecycle()
    val ligas by viewModel.ligas.collectAsStateWithLifecycle()
    val propostos by viewModel.propostos.collectAsStateWithLifecycle()
    val aviso by viewModel.aviso.collectAsStateWithLifecycle()
    val aGuardar by viewModel.aGuardar.collectAsStateWithLifecycle()
    var alvo by remember { mutableStateOf<MarketPlayerDto?>(null) }

    alvo?.let { j ->
        DialogoProposta(
            jogador = j,
            aoConfirmar = { msg -> viewModel.proporTransferencia(j, msg); alvo = null },
            aoFechar = { alvo = null }
        )
    }

    LoadingPage(
        isLoading = aCarregar,
        errorMsg = erro,
        retry = viewModel::pesquisar,
        content = {
            LazyColumn(modifier = Modifier
                .fillMaxSize()
                .padding(horizontal = 16.dp)) {
                item {
                    Text("Mercado", style = MaterialTheme.typography.headlineSmall, fontWeight = FontWeight.Bold,
                        modifier = Modifier.padding(top = 16.dp))
                    Text(
                        "Jogadores de outras equipas e jogadores livres. Não há dinheiro envolvido: " +
                            "a transferência acontece quando o clube e o jogador aceitam.",
                        style = MaterialTheme.typography.bodySmall,
                        color = MaterialTheme.colorScheme.onSurfaceVariant
                    )
                    FiltrosMercadoUi(filtros, ligas, viewModel::mudarFiltros, viewModel::pesquisar, viewModel::limpar, aGuardar)
                    AvisoCompeticao(aviso?.texto, aviso?.erro == true)
                    if (jogadores.isEmpty()) {
                        Text("Nenhum jogador encontrado com estes filtros.", modifier = Modifier.padding(vertical = 16.dp))
                    }
                }
                items(jogadores, key = { it.playerId }) { j ->
                    CartaoCompeticao {
                        Text(j.name, style = MaterialTheme.typography.titleMedium, fontWeight = FontWeight.Bold)
                        Text(
                            "${Textos.posicao(j.position)} · ${j.age} anos · ${j.nationality ?: "nacionalidade por indicar"}",
                            style = MaterialTheme.typography.bodySmall
                        )
                        Text(
                            if (j.teamId == null) "Jogador livre" else "${j.teamName} · ${j.leagueName ?: "sem liga"}",
                            style = MaterialTheme.typography.bodySmall,
                            color = MaterialTheme.colorScheme.onSurfaceVariant
                        )
                        if (j.isListed) {
                            Text("No mercado (o clube aceita propostas)", style = MaterialTheme.typography.labelSmall,
                                color = CoresCompeticao.VITORIA)
                        }
                        Row(horizontalArrangement = Arrangement.spacedBy(8.dp), modifier = Modifier.padding(top = 8.dp)) {
                            Button(onClick = { alvo = j }, enabled = !aGuardar && j.playerId !in propostos) {
                                Text(if (j.playerId in propostos) "Proposta enviada" else "Fazer proposta")
                            }
                            OutlinedButton(onClick = {
                                navHostController.navigate("${Routes.UserRoutes.PROFILE.route}/${j.playerId}")
                            }) { Text("Ver perfil") }
                        }
                    }
                }
            }
        }
    )
}

@Composable
private fun FiltrosMercadoUi(
    f: FiltrosMercado,
    ligas: List<LeagueDto>,
    mudar: (FiltrosMercado) -> Unit,
    pesquisar: () -> Unit,
    limpar: () -> Unit,
    aGuardar: Boolean
) {
    val opcoesEquipa = listOf<Boolean?>(null, true, false)
    val opcoesLiga = listOf<LeagueDto?>(null) + ligas
    val opcoesPosicao = listOf<Int?>(null, 0, 1, 2, 3)
    Column(modifier = Modifier.padding(top = 8.dp), verticalArrangement = Arrangement.spacedBy(8.dp)) {
        OutlinedTextField(
            value = f.nome, onValueChange = { mudar(f.copy(nome = it)) },
            label = { Text("Nome") }, singleLine = true, modifier = Modifier.fillMaxWidth()
        )
        OutlinedTextField(
            value = f.nacionalidade, onValueChange = { mudar(f.copy(nacionalidade = it)) },
            label = { Text("Nacionalidade") }, singleLine = true, modifier = Modifier.fillMaxWidth()
        )
        Row(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
            SelectBox(
                list = opcoesEquipa,
                selectedValue = f.temEquipa,
                onSelectItem = { mudar(f.copy(temEquipa = it)) },
                itemToString = { when (it) { null -> "Com e sem equipa"; true -> "Com equipa"; false -> "Livres" } },
                modifier = Modifier.weight(1f)
            )
            SelectBox(
                list = opcoesPosicao,
                selectedValue = f.posicao,
                onSelectItem = { mudar(f.copy(posicao = it)) },
                itemToString = { if (it == null) "Todas as posições" else Textos.posicao(it) },
                modifier = Modifier.weight(1f)
            )
        }
        SelectBox(
            list = opcoesLiga,
            selectedValue = opcoesLiga.firstOrNull { it?.id == f.ligaId },
            onSelectItem = { mudar(f.copy(ligaId = it?.id)) },
            itemToString = { it?.name ?: "Todas as ligas" },
            modifier = Modifier.fillMaxWidth()
        )
        Row(verticalAlignment = Alignment.CenterVertically) {
            Checkbox(checked = f.soListados, onCheckedChange = { mudar(f.copy(soListados = it)) })
            Text("Só jogadores colocados no mercado")
        }
        Row {
            Button(onClick = pesquisar, enabled = !aGuardar) { Text("Pesquisar") }
            Spacer(Modifier.width(8.dp))
            OutlinedButton(onClick = limpar, enabled = !aGuardar) { Text("Limpar") }
        }
    }
}

@Composable
private fun DialogoProposta(jogador: MarketPlayerDto, aoConfirmar: (String) -> Unit, aoFechar: () -> Unit) {
    var mensagem by remember { mutableStateOf("") }
    AlertDialog(
        onDismissRequest = aoFechar,
        title = { Text("Proposta a ${jogador.name}") },
        text = {
            Column {
                Text(
                    when {
                        jogador.teamId == null -> "É um jogador livre: basta ele aceitar."
                        jogador.isListed -> "O clube colocou-o no mercado: basta o jogador aceitar."
                        else -> "Primeiro decide o clube (${jogador.teamName}), depois o jogador."
                    }
                )
                OutlinedTextField(
                    value = mensagem,
                    onValueChange = { if (it.length <= 200) mensagem = it },
                    label = { Text("Mensagem (opcional)") },
                    modifier = Modifier.fillMaxWidth().padding(top = 8.dp)
                )
            }
        },
        confirmButton = { TextButton(onClick = { aoConfirmar(mensagem) }) { Text("Enviar proposta") } },
        dismissButton = { TextButton(onClick = aoFechar) { Text("Cancelar") } }
    )
}
