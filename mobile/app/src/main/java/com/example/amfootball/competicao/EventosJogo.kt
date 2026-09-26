package com.example.amfootball.competicao

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Delete
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedButton
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.unit.dp
import androidx.hilt.navigation.compose.hiltViewModel
import androidx.lifecycle.SavedStateHandle
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import com.example.amfootball.core.utils.Arguments
import com.example.amfootball.data.filters.FilterMembersTeam
import com.example.amfootball.data.local.SessionManager
import com.example.amfootball.data.remote.services.TeamService
import com.example.amfootball.ui.components.inputFields.SelectBox
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import javax.inject.Inject
import javax.inject.Singleton

/**
 * Guarda os eventos preenchidos no formulário de fim de jogo até o lobby do SignalR os enviar
 * com o resultado (a navegação só leva números no URL).
 */
@Singleton
class ResultadoPendente @Inject constructor() {
    private val eventos = mutableMapOf<String, MatchEventsDto>()

    fun guardar(jogoId: String, e: MatchEventsDto) {
        eventos[jogoId] = e
    }

    fun obter(jogoId: String): MatchEventsDto? = eventos[jogoId]
}

/** Linhas editáveis (texto) antes de serem convertidas para o DTO. */
data class LinhaGolo(val marcador: String? = null, val assistente: String? = null, val minuto: String = "")
data class LinhaCartao(val jogador: String? = null, val vermelho: Boolean = false, val minuto: String = "")
data class LinhaSubstituicao(val sai: String? = null, val entra: String? = null, val minuto: String = "")

data class FormEventos(
    val faltas: String = "0",
    val golos: List<LinhaGolo> = emptyList(),
    val cartoes: List<LinhaCartao> = emptyList(),
    val substituicoes: List<LinhaSubstituicao> = emptyList()
)

@HiltViewModel
class EventosJogoViewModel @Inject constructor(
    private val servico: CompeticaoService,
    private val equipas: TeamService,
    private val pendente: ResultadoPendente,
    sessao: SessionManager,
    savedStateHandle: SavedStateHandle
) : CompeticaoViewModel() {

    private val jogoId: String = savedStateHandle.get<String>(Arguments.MATCH_ID) ?: ""
    private val equipaId: String = sessao.fetchTeamId()

    private val _jogadores = MutableStateFlow<List<JogadorPlantel>>(emptyList())
    val jogadores: StateFlow<List<JogadorPlantel>> = _jogadores.asStateFlow()

    private val _onze = MutableStateFlow<LineupDto?>(null)
    val onze: StateFlow<LineupDto?> = _onze.asStateFlow()

    private val _taticas = MutableStateFlow<List<FormationDto>>(emptyList())
    val taticas: StateFlow<List<FormationDto>> = _taticas.asStateFlow()

    private val _form = MutableStateFlow(FormEventos())
    val form: StateFlow<FormEventos> = _form.asStateFlow()

    init {
        carregar {
            // Com onze: só os convocados (titulares e banco). Sem onze: todo o plantel.
            val onze = try { servico.onze(jogoId, equipaId) } catch (e: Exception) { null }
            _onze.value = onze?.takeIf { !it.starters.isNullOrEmpty() }
            _taticas.value = try { servico.taticas() } catch (e: Exception) { emptyList() }
            val convocados = onze?.starters.orEmpty().mapNotNull { s ->
                s.playerId?.let { JogadorPlantel(it, s.playerName ?: "?", s.position) }
            } + onze?.bench.orEmpty().map { JogadorPlantel(it.playerId, it.playerName, it.position) }
            _jogadores.value = convocados.ifEmpty {
                equipas.getListMembers(equipaId, FilterMembersTeam()).map { JogadorPlantel(it.id, it.name, it.positionId) }
            }
        }
    }

    fun mudar(f: FormEventos) {
        _form.value = f
    }

    /** Converte, valida e guarda para o envio. Devolve a mensagem de erro ou nulo. */
    fun preparar(golosEquipa: Int): String? {
        val f = _form.value
        fun minuto(t: String) = t.trim().toIntOrNull() ?: 0
        if (f.cartoes.any { it.jogador == null } || f.substituicoes.any { it.sai == null || it.entra == null }) {
            return "Escolhe os jogadores de todos os cartões e substituições (ou apaga as linhas vazias)."
        }
        val eventos = MatchEventsDto(
            fouls = f.faltas.trim().toIntOrNull() ?: 0,
            goals = f.golos.map { GoalEventDto(it.marcador, it.assistente, minuto(it.minuto)) },
            cards = f.cartoes.map { CardEventDto(it.jogador!!, if (it.vermelho) 1 else 0, minuto(it.minuto)) },
            substitutions = f.substituicoes.map { SubstitutionEventDto(it.sai!!, it.entra!!, minuto(it.minuto)) }
        )
        val titulares = _onze.value?.starters.orEmpty().mapNotNull { it.playerId }.toSet()
        RegrasEventos.validar(golosEquipa, eventos, titulares)?.let { return it }
        pendente.guardar(jogoId, eventos)
        return null
    }
}

/**
 * Secção do formulário de fim de jogo com faltas, golos (marcador, assistência e minuto), cartões
 * e substituições. Cada administrador regista os eventos da sua equipa.
 */
@Composable
fun SeccaoEventosJogo(golosEquipa: Int, viewModel: EventosJogoViewModel) {
    val jogadores by viewModel.jogadores.collectAsStateWithLifecycle()
    val f by viewModel.form.collectAsStateWithLifecycle()
    val onze by viewModel.onze.collectAsStateWithLifecycle()
    val taticas by viewModel.taticas.collectAsStateWithLifecycle()
    val opcoes: List<String?> = listOf<String?>(null) + jogadores.map { it.id }
    val nome: (String?) -> String = { id -> jogadores.firstOrNull { it.id == id }?.nome ?: "—" }

    Column(modifier = Modifier.fillMaxWidth()) {
        TituloSeccao("Eventos da tua equipa")
        onze?.let { o ->
            Text("Onze: tática ${o.formation}", style = MaterialTheme.typography.bodySmall)
            CampoFutebol(pontos = pontosDoOnze(o, taticas), modifier = Modifier.padding(vertical = 8.dp))
        }

        OutlinedTextField(
            value = f.faltas,
            onValueChange = { v -> viewModel.mudar(f.copy(faltas = v.filter { it.isDigit() }.take(3))) },
            label = { Text("Faltas cometidas") },
            keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number),
            singleLine = true,
            modifier = Modifier.fillMaxWidth()
        )

        Subtitulo("Golos (${f.golos.size} de $golosEquipa)")
        f.golos.forEachIndexed { i, g ->
            Linha(
                minuto = g.minuto,
                aoMudarMinuto = { m -> viewModel.mudar(f.copy(golos = f.golos.troca(i, g.copy(minuto = m)))) },
                aoApagar = { viewModel.mudar(f.copy(golos = f.golos.filterIndexed { j, _ -> j != i })) }
            ) {
                SelectBox(opcoes, g.marcador, { viewModel.mudar(f.copy(golos = f.golos.troca(i, g.copy(marcador = it)))) },
                    { if (it == null) "Marcador (desconhecido)" else nome(it) })
                SelectBox(opcoes, g.assistente, { viewModel.mudar(f.copy(golos = f.golos.troca(i, g.copy(assistente = it)))) },
                    { if (it == null) "Sem assistência" else "Assist.: ${nome(it)}" })
            }
        }
        if (f.golos.size < golosEquipa) {
            OutlinedButton(onClick = { viewModel.mudar(f.copy(golos = f.golos + LinhaGolo())) }) { Text("Adicionar golo") }
        }

        Subtitulo("Cartões (${f.cartoes.count { !it.vermelho }} amarelos, ${f.cartoes.count { it.vermelho }} vermelhos)")
        f.cartoes.forEachIndexed { i, c ->
            Linha(
                minuto = c.minuto,
                aoMudarMinuto = { m -> viewModel.mudar(f.copy(cartoes = f.cartoes.troca(i, c.copy(minuto = m)))) },
                aoApagar = { viewModel.mudar(f.copy(cartoes = f.cartoes.filterIndexed { j, _ -> j != i })) }
            ) {
                SelectBox(opcoes, c.jogador, { viewModel.mudar(f.copy(cartoes = f.cartoes.troca(i, c.copy(jogador = it)))) },
                    { if (it == null) "Escolher jogador" else nome(it) })
                SelectBox(listOf(false, true), c.vermelho, { viewModel.mudar(f.copy(cartoes = f.cartoes.troca(i, c.copy(vermelho = it)))) },
                    { if (it) "Vermelho" else "Amarelo" })
            }
        }
        OutlinedButton(onClick = { viewModel.mudar(f.copy(cartoes = f.cartoes + LinhaCartao())) }) { Text("Adicionar cartão") }

        Subtitulo("Substituições (${f.substituicoes.size})")
        f.substituicoes.forEachIndexed { i, s ->
            Linha(
                minuto = s.minuto,
                aoMudarMinuto = { m -> viewModel.mudar(f.copy(substituicoes = f.substituicoes.troca(i, s.copy(minuto = m)))) },
                aoApagar = { viewModel.mudar(f.copy(substituicoes = f.substituicoes.filterIndexed { j, _ -> j != i })) }
            ) {
                SelectBox(opcoes, s.sai, { viewModel.mudar(f.copy(substituicoes = f.substituicoes.troca(i, s.copy(sai = it)))) },
                    { if (it == null) "Sai" else "Sai: ${nome(it)}" })
                SelectBox(opcoes, s.entra, { viewModel.mudar(f.copy(substituicoes = f.substituicoes.troca(i, s.copy(entra = it)))) },
                    { if (it == null) "Entra" else "Entra: ${nome(it)}" })
            }
        }
        OutlinedButton(onClick = { viewModel.mudar(f.copy(substituicoes = f.substituicoes + LinhaSubstituicao())) }) {
            Text("Adicionar substituição")
        }
    }
}

@Composable
private fun Subtitulo(texto: String) {
    Text(texto, fontWeight = FontWeight.Bold, modifier = Modifier.padding(top = 16.dp, bottom = 4.dp))
}

@Composable
private fun Linha(
    minuto: String,
    aoMudarMinuto: (String) -> Unit,
    aoApagar: () -> Unit,
    campos: @Composable () -> Unit
) {
    CartaoCompeticao {
        Column(verticalArrangement = Arrangement.spacedBy(6.dp)) { campos() }
        Row(verticalAlignment = Alignment.CenterVertically) {
            OutlinedTextField(
                value = minuto,
                onValueChange = { v -> aoMudarMinuto(v.filter { it.isDigit() }.take(3)) },
                label = { Text("Minuto") },
                keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number),
                singleLine = true,
                modifier = Modifier.width(110.dp)
            )
            IconButton(onClick = aoApagar) { Icon(Icons.Filled.Delete, contentDescription = "Apagar linha") }
        }
    }
}

private fun <T> List<T>.troca(i: Int, novo: T): List<T> = mapIndexed { j, v -> if (j == i) novo else v }
