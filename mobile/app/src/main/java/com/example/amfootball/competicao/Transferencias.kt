package com.example.amfootball.competicao

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.material3.Button
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedButton
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.hilt.navigation.compose.hiltViewModel
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import androidx.navigation.NavHostController
import com.example.amfootball.data.local.SessionManager
import com.example.amfootball.ui.components.LoadingPage
import com.example.amfootball.ui.navigation.objects.Routes
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import javax.inject.Inject

/** Propostas da equipa (antes "pedidos de adesão"): as recebidas pelos nossos jogadores e as que fizemos. */
@HiltViewModel
class TransferenciasEquipaViewModel @Inject constructor(
    private val servico: CompeticaoService,
    sessao: SessionManager
) : CompeticaoViewModel() {
    private val equipaId: String = sessao.getUserProfile()?.effectiveTeamId ?: ""

    private val _propostas = MutableStateFlow(TeamOffersDto())
    val propostas: StateFlow<TeamOffersDto> = _propostas.asStateFlow()

    init {
        recarregar()
    }

    fun recarregar() {
        carregar { _propostas.value = servico.propostasEquipa(equipaId) }
    }

    fun aceitar(p: TransferOfferDto) = acao("Aceitaste a proposta: agora falta a resposta de ${p.playerName}.") {
        servico.aceitarProposta(p.id)
        _propostas.value = servico.propostasEquipa(equipaId)
    }

    fun recusar(p: TransferOfferDto, cancelar: Boolean) =
        acao(if (cancelar) "Proposta cancelada." else "Proposta recusada.") {
            servico.recusarProposta(p.id)
            _propostas.value = servico.propostasEquipa(equipaId)
        }
}

/** Propostas à espera da resposta do jogador autenticado. */
@HiltViewModel
class PropostasJogadorViewModel @Inject constructor(
    private val servico: CompeticaoService,
    private val sessao: SessionManager
) : CompeticaoViewModel() {
    private val _propostas = MutableStateFlow<List<TransferOfferDto>>(emptyList())
    val propostas: StateFlow<List<TransferOfferDto>> = _propostas.asStateFlow()

    init {
        recarregar()
    }

    fun recarregar() {
        carregar { _propostas.value = servico.propostasJogador() }
    }

    fun aceitar(p: TransferOfferDto) =
        acao("Bem-vindo ao ${p.toTeamName}!") {
            servico.aceitarProposta(p.id)
            // A sessão passa a apontar para a equipa nova (sem o objeto da equipa antiga).
            sessao.getUserProfile()?.let {
                sessao.saveUserProfile(it.copy(idTeam = p.toTeamId, team = null, isAdmin = false))
            }
            _propostas.value = servico.propostasJogador()
        }

    fun recusar(p: TransferOfferDto) = acao("Proposta recusada.") {
        servico.recusarProposta(p.id)
        _propostas.value = servico.propostasJogador()
    }
}

@Composable
fun TransferenciasEquipaScreen(
    navHostController: NavHostController,
    viewModel: TransferenciasEquipaViewModel = hiltViewModel()
) {
    val aCarregar by viewModel.aCarregar.collectAsStateWithLifecycle()
    val erro by viewModel.erroCarga.collectAsStateWithLifecycle()
    val propostas by viewModel.propostas.collectAsStateWithLifecycle()
    val aviso by viewModel.aviso.collectAsStateWithLifecycle()
    val aGuardar by viewModel.aGuardar.collectAsStateWithLifecycle()

    LoadingPage(isLoading = aCarregar, errorMsg = erro, retry = viewModel::recarregar, content = {
        val recebidas = propostas.received.orEmpty()
        val enviadas = propostas.sent.orEmpty()
        LazyColumn(modifier = Modifier
            .fillMaxSize()
            .padding(horizontal = 16.dp)) {
            item {
                Text("Transferências", style = MaterialTheme.typography.headlineSmall, fontWeight = FontWeight.Bold,
                    modifier = Modifier.padding(top = 16.dp))
                Text(
                    "Um jogador muda de equipa quando o clube dele e o próprio jogador aceitam. " +
                        "Os jogadores no mercado já têm o acordo do clube.",
                    style = MaterialTheme.typography.bodySmall,
                    color = MaterialTheme.colorScheme.onSurfaceVariant
                )
                Row(horizontalArrangement = Arrangement.spacedBy(8.dp), modifier = Modifier.padding(top = 8.dp)) {
                    OutlinedButton(onClick = { navHostController.navigate(RotasCompeticao.MERCADO) }) { Text("Ir ao mercado") }
                }
                AvisoCompeticao(aviso?.texto, aviso?.erro == true)
                TituloSeccao("Recebidas (${recebidas.size})")
                if (recebidas.isEmpty()) Text("Não há propostas pelos teus jogadores.")
            }
            items(recebidas, key = { "r" + it.id }) { p ->
                CartaoProposta(p, "De ${p.toTeamName} por ${p.playerName}") {
                    if (p.status == 0) {
                        Button(onClick = { viewModel.aceitar(p) }, enabled = !aGuardar) { Text("Aceitar") }
                        OutlinedButton(onClick = { viewModel.recusar(p, cancelar = false) }, enabled = !aGuardar) { Text("Recusar") }
                    }
                }
            }
            item {
                TituloSeccao("Enviadas (${enviadas.size})")
                if (enviadas.isEmpty()) Text("Ainda não fizeste propostas.")
            }
            items(enviadas, key = { "e" + it.id }) { p ->
                CartaoProposta(p, "${p.playerName} (${p.fromTeamName ?: "jogador livre"})") {
                    if (p.status == 0 || p.status == 1) {
                        OutlinedButton(onClick = { viewModel.recusar(p, cancelar = true) }, enabled = !aGuardar) { Text("Cancelar") }
                    }
                }
            }
        }
    })
}

@Composable
fun PropostasJogadorScreen(
    navHostController: NavHostController,
    viewModel: PropostasJogadorViewModel = hiltViewModel()
) {
    val aCarregar by viewModel.aCarregar.collectAsStateWithLifecycle()
    val erro by viewModel.erroCarga.collectAsStateWithLifecycle()
    val propostas by viewModel.propostas.collectAsStateWithLifecycle()
    val aviso by viewModel.aviso.collectAsStateWithLifecycle()
    val aGuardar by viewModel.aGuardar.collectAsStateWithLifecycle()

    LoadingPage(isLoading = aCarregar, errorMsg = erro, retry = viewModel::recarregar, content = {
        LazyColumn(modifier = Modifier
            .fillMaxSize()
            .padding(horizontal = 16.dp)) {
            item {
                Text("Propostas para mim", style = MaterialTheme.typography.headlineSmall, fontWeight = FontWeight.Bold,
                    modifier = Modifier.padding(top = 16.dp))
                Text(
                    "Estas propostas já têm o acordo do teu clube (ou és jogador livre). Se aceitares, mudas de equipa.",
                    style = MaterialTheme.typography.bodySmall,
                    color = MaterialTheme.colorScheme.onSurfaceVariant
                )
                AvisoCompeticao(aviso?.texto, aviso?.erro == true)
                if (propostas.isEmpty()) Text("Não tens propostas por responder.", modifier = Modifier.padding(vertical = 16.dp))
            }
            items(propostas, key = { it.id }) { p ->
                CartaoProposta(p, "${p.toTeamName} quer contar contigo") {
                    Button(onClick = { viewModel.aceitar(p) }, enabled = !aGuardar) { Text("Aceitar") }
                    OutlinedButton(onClick = { viewModel.recusar(p) }, enabled = !aGuardar) { Text("Recusar") }
                    OutlinedButton(onClick = {
                        navHostController.navigate("${Routes.TeamRoutes.TEAM_PROFILE.route}/${p.toTeamId}")
                    }) { Text("Ver equipa") }
                }
            }
        }
    })
}

@Composable
private fun CartaoProposta(p: TransferOfferDto, titulo: String, botoes: @Composable () -> Unit) {
    CartaoCompeticao {
        Text(titulo, style = MaterialTheme.typography.titleSmall, fontWeight = FontWeight.Bold)
        Text(
            "${Textos.estadoProposta(p.status)} · ${Datas.data(p.createdAt)}",
            style = MaterialTheme.typography.bodySmall,
            color = MaterialTheme.colorScheme.onSurfaceVariant
        )
        p.message?.takeIf { it.isNotBlank() }?.let { Text("“$it”", style = MaterialTheme.typography.bodyMedium) }
        Row(horizontalArrangement = Arrangement.spacedBy(8.dp), modifier = Modifier.padding(top = 8.dp)) { botoes() }
    }
}
