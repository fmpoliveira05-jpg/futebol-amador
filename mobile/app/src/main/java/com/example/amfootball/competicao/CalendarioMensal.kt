package com.example.amfootball.competicao

import android.app.DatePickerDialog
import android.app.TimePickerDialog
import androidx.compose.foundation.background
import androidx.compose.foundation.border
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.ExperimentalLayoutApi
import androidx.compose.foundation.layout.FlowRow
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.aspectRatio
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.KeyboardArrowLeft
import androidx.compose.material.icons.automirrored.filled.KeyboardArrowRight
import androidx.compose.material3.AlertDialog
import androidx.compose.material3.Button
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
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
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import com.example.amfootball.data.local.SessionManager
import com.example.amfootball.data.remote.dtos.match.InfoMatchCalendar
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import java.time.LocalDate
import java.time.LocalDateTime
import java.time.LocalTime
import java.time.YearMonth
import javax.inject.Inject

@HiltViewModel
class CalendarioMensalViewModel @Inject constructor(
    private val servico: CompeticaoService,
    sessao: SessionManager
) : CompeticaoViewModel() {
    private val equipaId: String = sessao.fetchTeamId()
    val souAdmin: Boolean = sessao.getUserProfile()?.isAdmin == true

    private val _mes = MutableStateFlow(YearMonth.now())
    val mes: StateFlow<YearMonth> = _mes.asStateFlow()

    private val _dia = MutableStateFlow<LocalDate?>(LocalDate.now())
    val dia: StateFlow<LocalDate?> = _dia.asStateFlow()

    private val _historico = MutableStateFlow<List<CalendarMarkerDto>>(emptyList())
    val historico: StateFlow<List<CalendarMarkerDto>> = _historico.asStateFlow()

    init {
        recarregarHistorico()
    }

    fun recarregarHistorico() {
        if (equipaId.isBlank()) return
        acao(null) { _historico.value = servico.historicoCalendario(equipaId) }
    }

    fun mesAnterior() {
        _mes.value = _mes.value.minusMonths(1)
    }

    fun mesSeguinte() {
        _mes.value = _mes.value.plusMonths(1)
    }

    fun hoje() {
        _mes.value = YearMonth.now()
        _dia.value = LocalDate.now()
    }

    fun escolherDia(d: LocalDate) {
        _dia.value = d
        if (YearMonth.from(d) != _mes.value) _mes.value = YearMonth.from(d)
    }

    /** Jogo da liga: cancela e remarca de uma vez (um jogo da liga não fica por jogar). */
    fun cancelarJogoLiga(jogoId: String, motivo: String, novaData: LocalDateTime, depois: () -> Unit): String? {
        val m = motivo.trim()
        if (m.length < 3 || m.length > 50) return "O motivo tem de ter entre 3 e 50 caracteres."
        if (!novaData.isAfter(LocalDateTime.now())) return "A nova data tem de ser no futuro."
        acao("Jogo cancelado e remarcado para ${Datas.dataHora(novaData)}.") {
            servico.cancelarERemarcar(equipaId, jogoId, m, novaData.withSecond(0).withNano(0).toString())
            _historico.value = servico.historicoCalendario(equipaId)
            depois()
        }
        return null
    }
}

/** Ações disponíveis nos jogos do dia escolhido. */
data class AcoesJogoCalendario(
    val souAdmin: Boolean,
    val aoAbrirOnze: (String) -> Unit,
    val aoAbrirRelatorio: (String) -> Unit,
    val aoIniciar: (String, String) -> Unit,
    val aoTerminar: (String, String) -> Unit,
    val aoAdiar: (String) -> Unit,
    val aoCancelar: (InfoMatchCalendar) -> Unit
)

/**
 * Calendário em grelha mensal com setas entre meses e bolinhas por dia: cinzento feriado, azul
 * amigável marcado, roxo jogo da liga marcado, verde terminado, vermelho cancelado e amarelo adiado.
 */
@Composable
fun CalendarioMensal(
    jogos: List<InfoMatchCalendar>,
    viewModel: CalendarioMensalViewModel,
    acoes: AcoesJogoCalendario
) {
    val mes by viewModel.mes.collectAsStateWithLifecycle()
    val diaEscolhido by viewModel.dia.collectAsStateWithLifecycle()
    val historico by viewModel.historico.collectAsStateWithLifecycle()
    val aviso by viewModel.aviso.collectAsStateWithLifecycle()

    val feriados = remember(mes.year) { Feriados.doAno(mes.year - 1) + Feriados.doAno(mes.year) + Feriados.doAno(mes.year + 1) }
    val jogosPorDia = remember(jogos) { jogos.groupBy { it.gameDate.toLocalDate() } }
    val historicoPorDia = remember(historico) {
        historico.mapNotNull { h -> Datas.parse(h.date)?.toLocalDate()?.let { it to h } }.groupBy({ it.first }, { it.second })
    }

    Column(modifier = Modifier.fillMaxWidth()) {
        Row(verticalAlignment = Alignment.CenterVertically, modifier = Modifier.fillMaxWidth()) {
            IconButton(onClick = viewModel::mesAnterior) {
                Icon(Icons.AutoMirrored.Filled.KeyboardArrowLeft, contentDescription = "Mês anterior")
            }
            Text(
                GrelhaMes.titulo(mes),
                style = MaterialTheme.typography.titleMedium,
                fontWeight = FontWeight.Bold,
                textAlign = TextAlign.Center,
                modifier = Modifier.weight(1f)
            )
            IconButton(onClick = viewModel::mesSeguinte) {
                Icon(Icons.AutoMirrored.Filled.KeyboardArrowRight, contentDescription = "Mês seguinte")
            }
        }
        TextButton(onClick = viewModel::hoje, modifier = Modifier.align(Alignment.CenterHorizontally)) { Text("Hoje") }

        Row(modifier = Modifier.fillMaxWidth()) {
            GrelhaMes.DIAS_SEMANA.forEach {
                Text(it, modifier = Modifier.weight(1f), textAlign = TextAlign.Center,
                    style = MaterialTheme.typography.labelSmall, fontWeight = FontWeight.Bold)
            }
        }
        GrelhaMes.semanas(mes).forEach { semana ->
            Row(modifier = Modifier.fillMaxWidth()) {
                semana.forEach { d ->
                    val marcas = MarcasCalendario.doDia(
                        feriado = feriados.containsKey(d),
                        jogos = jogosPorDia[d].orEmpty().map { MarcasCalendario.tipoDoJogo(it.matchStatusId, it.typeMatchBool) },
                        historico = historicoPorDia[d].orEmpty().map { MarcasCalendario.tipoDoHistorico(it.kind) }
                    )
                    CelulaDia(
                        dia = d,
                        doMes = YearMonth.from(d) == mes,
                        hoje = d == LocalDate.now(),
                        escolhido = d == diaEscolhido,
                        marcas = marcas,
                        modifier = Modifier.weight(1f),
                        aoTocar = { viewModel.escolherDia(d) }
                    )
                }
            }
        }
        LegendaCalendario()
        AvisoCompeticao(aviso?.texto, aviso?.erro == true)

        diaEscolhido?.let { d ->
            DetalheDia(
                dia = d,
                feriado = feriados[d],
                jogos = jogosPorDia[d].orEmpty().sortedBy { it.gameDate },
                historico = historicoPorDia[d].orEmpty(),
                acoes = acoes
            )
        }
    }
}

@Composable
private fun CelulaDia(
    dia: LocalDate,
    doMes: Boolean,
    hoje: Boolean,
    escolhido: Boolean,
    marcas: List<TipoMarca>,
    modifier: Modifier,
    aoTocar: () -> Unit
) {
    val fundo = when {
        escolhido -> MaterialTheme.colorScheme.primaryContainer
        else -> Color.Transparent
    }
    Column(
        modifier = modifier
            .aspectRatio(1f)
            .padding(2.dp)
            .background(fundo, RoundedCornerShape(6.dp))
            .let { if (hoje) it.border(1.dp, MaterialTheme.colorScheme.primary, RoundedCornerShape(6.dp)) else it }
            .clickable(onClick = aoTocar),
        horizontalAlignment = Alignment.CenterHorizontally,
        verticalArrangement = Arrangement.Center
    ) {
        Text(
            dia.dayOfMonth.toString(),
            style = MaterialTheme.typography.bodySmall,
            color = if (doMes) MaterialTheme.colorScheme.onSurface else MaterialTheme.colorScheme.outline,
            fontWeight = if (hoje) FontWeight.Bold else FontWeight.Normal
        )
        Row(horizontalArrangement = Arrangement.spacedBy(2.dp)) {
            marcas.take(4).forEach { Bolinha(CoresCompeticao.marca(it)) }
        }
    }
}

@OptIn(ExperimentalLayoutApi::class)
@Composable
private fun LegendaCalendario() {
    FlowRow(
        horizontalArrangement = Arrangement.spacedBy(12.dp),
        modifier = Modifier.padding(vertical = 8.dp)
    ) {
        TipoMarca.entries.forEach { t ->
            Row(verticalAlignment = Alignment.CenterVertically, modifier = Modifier.padding(vertical = 2.dp)) {
                Bolinha(CoresCompeticao.marca(t), 10)
                Spacer(Modifier.width(4.dp))
                Text(t.texto, style = MaterialTheme.typography.labelSmall)
            }
        }
    }
}

@Composable
private fun DetalheDia(
    dia: LocalDate,
    feriado: String?,
    jogos: List<InfoMatchCalendar>,
    historico: List<CalendarMarkerDto>,
    acoes: AcoesJogoCalendario
) {
    Column(modifier = Modifier.fillMaxWidth()) {
        TituloSeccao(Datas.data(dia.toString()))
        feriado?.let { Text("Feriado: $it", color = MaterialTheme.colorScheme.onSurfaceVariant) }
        if (jogos.isEmpty() && historico.isEmpty()) Text("Sem jogos neste dia.")

        jogos.forEach { j -> CartaoJogoDia(j, acoes) }

        historico.forEach { h ->
            CartaoCompeticao {
                val cancelado = h.kind == "CANCELLED"
                Text(
                    (if (cancelado) "Cancelado" else "Adiado") + " · contra ${h.opponentName ?: "?"}",
                    fontWeight = FontWeight.Bold,
                    color = CoresCompeticao.marca(MarcasCalendario.tipoDoHistorico(h.kind))
                        .let { if (cancelado) it else MaterialTheme.colorScheme.onSurface }
                )
                Text("Motivo: ${h.reason?.takeIf { it.isNotBlank() } ?: "não indicado"}")
                h.newDate?.let { Text("Nova data: ${Datas.dataHora(it)}") }
            }
        }
    }
}

@Composable
private fun CartaoJogoDia(j: InfoMatchCalendar, acoes: AcoesJogoCalendario) {
    val tipo = MarcasCalendario.tipoDoJogo(j.matchStatusId, j.typeMatchBool)
    CartaoCompeticao {
        Row(verticalAlignment = Alignment.CenterVertically) {
            Bolinha(CoresCompeticao.marca(tipo), 10)
            Spacer(Modifier.width(6.dp))
            Text(
                "${j.gameDate.toLocalTime().toString().take(5)} · ${j.team.name} vs ${j.opponent.name}",
                fontWeight = FontWeight.Bold
            )
        }
        Text(
            listOfNotNull(
                if (j.typeMatchBool) (j.leagueName ?: "Liga") else "Amigável (não conta pontos)",
                j.round?.let { "jornada $it" },
                Textos.estadoJogo(j.matchStatusId)
            ).joinToString(" · "),
            style = MaterialTheme.typography.bodySmall
        )
        if (j.matchStatusId == 2) Text("Resultado: ${j.team.numGoals} – ${j.opponent.numGoals}")
        if (j.pitchGame.name.isNotBlank()) Text("Campo: ${j.pitchGame.name}", style = MaterialTheme.typography.bodySmall)
        j.postponedFrom?.let { Text("Adiado de ${Datas.dataHora(it)}", style = MaterialTheme.typography.bodySmall) }
        j.reason?.takeIf { it.isNotBlank() }?.let {
            Text(
                (if (j.matchStatusId == 4) "Motivo do cancelamento: " else "Motivo do adiamento: ") + it,
                style = MaterialTheme.typography.bodySmall
            )
        }

        FlowRowBotoes {
            when (j.matchStatusId) {
                0, 3 -> {
                    OutlinedButton(onClick = { acoes.aoAbrirOnze(j.idMatch) }) { Text("Onze inicial") }
                    if (acoes.souAdmin) {
                        OutlinedButton(onClick = { acoes.aoIniciar(j.idMatch, j.opponent.idTeam) }) { Text("Iniciar") }
                        OutlinedButton(onClick = { acoes.aoTerminar(j.idMatch, j.opponent.idTeam) }) { Text("Terminar") }
                        OutlinedButton(onClick = { acoes.aoAdiar(j.idMatch) }) { Text("Adiar") }
                        OutlinedButton(onClick = { acoes.aoCancelar(j) }) {
                            Text(if (j.typeMatchBool) "Cancelar e remarcar" else "Cancelar")
                        }
                    }
                }
                1 -> {
                    if (acoes.souAdmin) {
                        Button(onClick = { acoes.aoTerminar(j.idMatch, j.opponent.idTeam) }) { Text("Terminar") }
                    }
                }
                2 -> OutlinedButton(onClick = { acoes.aoAbrirRelatorio(j.idMatch) }) { Text("Relatório do jogo") }
                else -> {}
            }
        }
    }
}

@OptIn(ExperimentalLayoutApi::class)
@Composable
private fun FlowRowBotoes(conteudo: @Composable () -> Unit) {
    FlowRow(
        horizontalArrangement = Arrangement.spacedBy(8.dp),
        modifier = Modifier.padding(top = 6.dp)
    ) { conteudo() }
}

/**
 * Cancelar um jogo da liga pede sempre uma nova data (o jogo não pode ficar por jogar).
 * As datas escolhem-se com os seletores do sistema.
 */
@Composable
fun DialogoCancelarJogoLiga(
    jogo: InfoMatchCalendar,
    aoConfirmar: (motivo: String, novaData: LocalDateTime) -> String?,
    aoFechar: () -> Unit
) {
    val contexto = LocalContext.current
    var motivo by remember { mutableStateOf("") }
    var data by remember { mutableStateOf(LocalDate.now().plusDays(7)) }
    var hora by remember { mutableStateOf(jogo.gameDate.toLocalTime().withSecond(0).withNano(0)) }
    var erro by remember { mutableStateOf<String?>(null) }

    AlertDialog(
        onDismissRequest = aoFechar,
        title = { Text("Cancelar e remarcar") },
        text = {
            Column(verticalArrangement = Arrangement.spacedBy(8.dp)) {
                Text("Um jogo da liga não pode ficar por jogar: indica o motivo e a nova data, combinada com o adversário.")
                OutlinedTextField(
                    value = motivo,
                    onValueChange = { if (it.length <= 50) motivo = it },
                    label = { Text("Motivo (3 a 50 caracteres)") }
                )
                OutlinedButton(onClick = {
                    DatePickerDialog(contexto, { _, a, m, d -> data = LocalDate.of(a, m + 1, d) },
                        data.year, data.monthValue - 1, data.dayOfMonth).show()
                }) { Text("Data: ${Datas.data(data.toString())}") }
                OutlinedButton(onClick = {
                    TimePickerDialog(contexto, { _, h, min -> hora = LocalTime.of(h, min) }, hora.hour, hora.minute, true).show()
                }) { Text("Hora: ${hora.toString().take(5)}") }
                erro?.let { Text(it, color = MaterialTheme.colorScheme.error) }
            }
        },
        confirmButton = {
            TextButton(onClick = { erro = aoConfirmar(motivo, LocalDateTime.of(data, hora)) }) { Text("Confirmar") }
        },
        dismissButton = { TextButton(onClick = aoFechar) { Text("Voltar") } }
    )
}
