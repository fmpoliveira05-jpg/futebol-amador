package com.example.amfootball.competicao

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.FilterChip
import androidx.compose.material3.HorizontalDivider
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import androidx.hilt.navigation.compose.hiltViewModel
import androidx.lifecycle.SavedStateHandle
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import com.example.amfootball.ui.components.LoadingPage
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import javax.inject.Inject

@HiltViewModel
class RelatorioViewModel @Inject constructor(
    private val servico: CompeticaoService,
    savedStateHandle: SavedStateHandle
) : CompeticaoViewModel() {
    private val jogoId: String = savedStateHandle.get<String>(RotasCompeticao.ARG_JOGO) ?: ""

    private val _relatorio = MutableStateFlow<MatchReportDto?>(null)
    val relatorio: StateFlow<MatchReportDto?> = _relatorio.asStateFlow()

    private val _taticas = MutableStateFlow<List<FormationDto>>(emptyList())
    val taticas: StateFlow<List<FormationDto>> = _taticas.asStateFlow()

    init {
        recarregar()
    }

    fun recarregar() {
        carregar {
            _relatorio.value = servico.relatorio(jogoId)
            _taticas.value = try { servico.taticas() } catch (e: Exception) { emptyList() }
        }
    }
}

@Composable
fun RelatorioScreen(viewModel: RelatorioViewModel = hiltViewModel()) {
    val aCarregar by viewModel.aCarregar.collectAsStateWithLifecycle()
    val erro by viewModel.erroCarga.collectAsStateWithLifecycle()
    val relatorio by viewModel.relatorio.collectAsStateWithLifecycle()
    val taticas by viewModel.taticas.collectAsStateWithLifecycle()
    var verVisitante by remember { mutableStateOf(false) }

    LoadingPage(isLoading = aCarregar, errorMsg = erro, retry = viewModel::recarregar, content = {
        val r = relatorio ?: return@LoadingPage
        val casa = r.home ?: TeamReportDto()
        val fora = r.away ?: TeamReportDto()
        Column(modifier = Modifier
            .fillMaxSize()
            .verticalScroll(rememberScrollState())
            .padding(16.dp)) {
            Text(
                listOfNotNull(
                    if (r.isCompetitive) (r.leagueName ?: "Liga") else "Amigável",
                    r.round?.let { "jornada $it" }
                ).joinToString(" · "),
                style = MaterialTheme.typography.labelLarge,
                color = MaterialTheme.colorScheme.primary
            )
            Row(verticalAlignment = Alignment.CenterVertically, modifier = Modifier
                .fillMaxWidth()
                .padding(vertical = 12.dp)) {
                Text(casa.teamName, modifier = Modifier.weight(1f), textAlign = TextAlign.End,
                    style = MaterialTheme.typography.titleMedium, fontWeight = FontWeight.Bold)
                Text(
                    if (casa.goals != null && fora.goals != null) "  ${casa.goals} – ${fora.goals}  " else "  vs  ",
                    style = MaterialTheme.typography.headlineMedium,
                    fontWeight = FontWeight.Bold
                )
                Text(fora.teamName, modifier = Modifier.weight(1f),
                    style = MaterialTheme.typography.titleMedium, fontWeight = FontWeight.Bold)
            }
            Text(
                "${Datas.dataHora(r.date)} · ${Textos.estadoJogo(r.status)}" + (r.pitchName?.let { " · $it" } ?: ""),
                style = MaterialTheme.typography.bodySmall,
                modifier = Modifier.fillMaxWidth(),
                textAlign = TextAlign.Center
            )
            if (!r.isCompetitive) {
                Text("Os amigáveis não contam pontos para a classificação.", style = MaterialTheme.typography.bodySmall,
                    modifier = Modifier.fillMaxWidth(), textAlign = TextAlign.Center)
            }

            TituloSeccao("Estatísticas")
            LinhaComparacao(casa.fouls, "Faltas", fora.fouls)
            LinhaComparacao(casa.yellowCards, "Amarelos", fora.yellowCards)
            LinhaComparacao(casa.redCards, "Vermelhos", fora.redCards)
            LinhaComparacao(casa.substitutions, "Substituições", fora.substitutions)

            TituloSeccao("Acontecimentos")
            val eventos = casa.events.orEmpty().map { it to true } + fora.events.orEmpty().map { it to false }
            if (eventos.isEmpty()) Text("Nenhuma das equipas registou acontecimentos.")
            eventos.sortedBy { it.first.minute }.forEach { (e, daCasa) ->
                Text(
                    text = "${e.minute}' ${descricaoEvento(e)}",
                    modifier = Modifier.fillMaxWidth().padding(vertical = 3.dp),
                    textAlign = if (daCasa) TextAlign.Start else TextAlign.End
                )
            }

            TituloSeccao("Onzes")
            Row(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                FilterChip(selected = !verVisitante, onClick = { verVisitante = false }, label = { Text(casa.teamName) })
                FilterChip(selected = verVisitante, onClick = { verVisitante = true }, label = { Text(fora.teamName) })
            }
            val equipa = if (verVisitante) fora else casa
            val onze = equipa.lineup
            if (onze == null || onze.starters.isNullOrEmpty()) {
                Text("Esta equipa não definiu o onze.", modifier = Modifier.padding(vertical = 8.dp))
            } else {
                Text("Tática ${onze.formation}" + if (onze.isAutoFilled) " (preenchido automaticamente)" else "",
                    style = MaterialTheme.typography.bodySmall, modifier = Modifier.padding(vertical = 6.dp))
                CampoFutebol(pontos = pontosDoOnze(onze, taticas))
                if (!onze.bench.isNullOrEmpty()) {
                    Text("Suplentes: " + onze.bench.joinToString(", ") { it.playerName },
                        style = MaterialTheme.typography.bodySmall, modifier = Modifier.padding(top = 6.dp))
                }
            }
        }
    })
}

@Composable
private fun LinhaComparacao(casa: Int, rotulo: String, fora: Int) {
    Column {
        Row(modifier = Modifier
            .fillMaxWidth()
            .padding(vertical = 6.dp)) {
            Text(casa.toString(), modifier = Modifier.weight(1f), fontWeight = FontWeight.Bold)
            Text(rotulo, modifier = Modifier.weight(2f), textAlign = TextAlign.Center)
            Text(fora.toString(), modifier = Modifier.weight(1f), textAlign = TextAlign.End, fontWeight = FontWeight.Bold)
        }
        HorizontalDivider()
    }
}

private fun descricaoEvento(e: ReportEventDto): String {
    val jogador = e.playerName ?: "Desconhecido"
    return when (e.type) {
        "GOAL" -> "⚽ $jogador" + (e.relatedPlayerName?.let { " (assist. $it)" } ?: "")
        "YELLOW_CARD" -> "🟨 $jogador"
        "RED_CARD" -> "🟥 $jogador"
        "SUBSTITUTION" -> "🔄 sai $jogador, entra ${e.relatedPlayerName ?: "?"}"
        else -> jogador
    }
}
