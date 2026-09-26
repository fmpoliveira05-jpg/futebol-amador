package com.example.amfootball.competicao

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
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
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import javax.inject.Inject

@HiltViewModel
class LigasViewModel @Inject constructor(
    private val servico: CompeticaoService,
    sessao: SessionManager
) : CompeticaoViewModel() {

    private val perfil = sessao.getUserProfile()
    val equipaId: String = perfil?.effectiveTeamId ?: ""
    val souAdmin: Boolean = equipaId.isNotBlank() && (perfil?.isAdmin == true)

    private val _ligas = MutableStateFlow<List<LeagueDto>>(emptyList())
    val ligas: StateFlow<List<LeagueDto>> = _ligas.asStateFlow()

    /** Jornadas carregadas por época (id da época → jornadas). */
    private val _jornadas = MutableStateFlow<Map<String, List<FixtureRoundDto>>>(emptyMap())
    val jornadas: StateFlow<Map<String, List<FixtureRoundDto>>> = _jornadas.asStateFlow()

    init {
        recarregar()
    }

    fun recarregar() {
        carregar { _ligas.value = servico.ligas().sortedBy { it.level } }
    }

    fun inscrever(liga: LeagueDto) {
        acao("A equipa ficou inscrita na ${liga.name}.") {
            servico.inscrever(liga.id, equipaId)
            _ligas.value = servico.ligas().sortedBy { it.level }
        }
    }

    fun alternarJornadas(epocaId: String) {
        if (_jornadas.value.containsKey(epocaId)) {
            _jornadas.value = _jornadas.value - epocaId
            return
        }
        acao(null) {
            _jornadas.value = _jornadas.value + (epocaId to servico.jornadas(epocaId))
        }
    }
}

@Composable
fun LigasScreen(
    navHostController: NavHostController,
    viewModel: LigasViewModel = hiltViewModel()
) {
    val aCarregar by viewModel.aCarregar.collectAsStateWithLifecycle()
    val erro by viewModel.erroCarga.collectAsStateWithLifecycle()
    val ligas by viewModel.ligas.collectAsStateWithLifecycle()
    val jornadas by viewModel.jornadas.collectAsStateWithLifecycle()
    val aviso by viewModel.aviso.collectAsStateWithLifecycle()
    val aGuardar by viewModel.aGuardar.collectAsStateWithLifecycle()

    LoadingPage(
        isLoading = aCarregar,
        errorMsg = erro,
        retry = viewModel::recarregar,
        content = {
            LazyColumn(
                modifier = Modifier
                    .fillMaxSize()
                    .padding(horizontal = 16.dp)
            ) {
                item {
                    Text("Ligas", style = MaterialTheme.typography.headlineSmall, fontWeight = FontWeight.Bold,
                        modifier = Modifier.padding(top = 16.dp))
                    Text(
                        "Os jogos da liga são sorteados a duas voltas, alternando casa e fora. " +
                            "No fim da época, o campeão recebe o troféu e há subidas e descidas de escalão.",
                        style = MaterialTheme.typography.bodySmall,
                        color = MaterialTheme.colorScheme.onSurfaceVariant
                    )
                    AvisoCompeticao(aviso?.texto, aviso?.erro == true)
                }
                if (ligas.isEmpty()) {
                    item { Text("Ainda não há ligas criadas.", modifier = Modifier.padding(vertical = 16.dp)) }
                }
                items(ligas, key = { it.id }) { liga ->
                    CartaoLiga(
                        liga = liga,
                        podeInscrever = viewModel.souAdmin && liga.currentSeason?.status == 0,
                        aGuardar = aGuardar,
                        jornadas = liga.currentSeason?.id?.let { jornadas[it] },
                        aoInscrever = { viewModel.inscrever(liga) },
                        aoVerJornadas = { liga.currentSeason?.id?.let(viewModel::alternarJornadas) },
                        aoAbrirJogo = { id -> navHostController.navigate("${RotasCompeticao.RELATORIO}/$id") }
                    )
                }
            }
        }
    )
}

@Composable
private fun CartaoLiga(
    liga: LeagueDto,
    podeInscrever: Boolean,
    aGuardar: Boolean,
    jornadas: List<FixtureRoundDto>?,
    aoInscrever: () -> Unit,
    aoVerJornadas: () -> Unit,
    aoAbrirJogo: (String) -> Unit
) {
    CartaoCompeticao {
        Text("${liga.name} · escalão ${liga.level}", style = MaterialTheme.typography.titleMedium, fontWeight = FontWeight.Bold)
        LinhaFicha("Troféu", liga.trophyName)
        LinhaFicha("Equipas", liga.teamCount.toString())
        LinhaFicha("Sobem / descem", "${liga.promotionSpots} / ${liga.relegationSpots}")
        LinhaFicha("Duração da época", "${liga.seasonDurationDays} dias")
        val epoca = liga.currentSeason
        if (epoca != null) {
            LinhaFicha("Época", "${epoca.name} · ${Textos.estadoEpoca(epoca.status)}")
            LinhaFicha("Datas", "${Datas.data(epoca.startDate)} a ${Datas.data(epoca.endDate)}")
        } else {
            LinhaFicha("Época", "Sem época aberta")
        }
        Row(horizontalArrangement = Arrangement.spacedBy(8.dp), modifier = Modifier.padding(top = 8.dp)) {
            if (podeInscrever) {
                Button(onClick = aoInscrever, enabled = !aGuardar) { Text("Inscrever a equipa") }
            }
            if (epoca != null && epoca.status != 0) {
                OutlinedButton(onClick = aoVerJornadas, enabled = !aGuardar) {
                    Text(if (jornadas == null) "Ver jornadas" else "Esconder jornadas")
                }
            }
        }
        jornadas?.let { ListaJornadas(it, aoAbrirJogo) }
    }
}

@Composable
private fun ListaJornadas(jornadas: List<FixtureRoundDto>, aoAbrirJogo: (String) -> Unit) {
    Column(modifier = Modifier.fillMaxWidth().padding(top = 8.dp)) {
        if (jornadas.isEmpty()) Text("O calendário ainda não foi sorteado.")
        jornadas.forEach { j ->
            Text("Jornada ${j.round}", fontWeight = FontWeight.Bold, modifier = Modifier.padding(top = 8.dp))
            j.matches.orEmpty().forEach { m ->
                val resultado = if (m.homeGoals != null && m.awayGoals != null) "${m.homeGoals} – ${m.awayGoals}" else "vs"
                OutlinedButton(onClick = { aoAbrirJogo(m.idMatch) }, modifier = Modifier.fillMaxWidth()) {
                    Column(modifier = Modifier.fillMaxWidth()) {
                        Text("${m.homeTeamName} $resultado ${m.awayTeamName}")
                        Text(
                            "${Datas.dataHora(m.date)} · ${Textos.estadoJogo(m.status)}",
                            style = MaterialTheme.typography.bodySmall
                        )
                    }
                }
            }
        }
    }
}
