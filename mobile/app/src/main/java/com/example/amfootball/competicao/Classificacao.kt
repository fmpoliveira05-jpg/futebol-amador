package com.example.amfootball.competicao

import android.util.Log
import androidx.compose.foundation.background
import androidx.compose.foundation.clickable
import androidx.compose.foundation.horizontalScroll
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.HorizontalDivider
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.unit.Dp
import androidx.compose.ui.unit.dp
import androidx.hilt.navigation.compose.hiltViewModel
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import androidx.navigation.NavHostController
import com.example.amfootball.data.NetworkConnectivityObserver
import com.example.amfootball.ui.components.LoadingPage
import com.example.amfootball.ui.components.inputFields.SelectBox
import com.example.amfootball.ui.components.notification.OfflineBanner
import com.example.amfootball.ui.navigation.objects.Routes
import com.google.firebase.firestore.FirebaseFirestore
import com.google.gson.Gson
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.tasks.await
import javax.inject.Inject

@HiltViewModel
class ClassificacaoViewModel @Inject constructor(
    private val servico: CompeticaoService,
    private val rede: NetworkConnectivityObserver,
    private val firestore: FirebaseFirestore
) : CompeticaoViewModel() {

    private val _ligas = MutableStateFlow<List<LeagueDto>>(emptyList())
    val ligas: StateFlow<List<LeagueDto>> = _ligas.asStateFlow()

    private val _tabela = MutableStateFlow<StandingsDto?>(null)
    val tabela: StateFlow<StandingsDto?> = _tabela.asStateFlow()

    private val _offline = MutableStateFlow(false)
    val offline: StateFlow<Boolean> = _offline.asStateFlow()

    private val gson = Gson()

    init {
        abrir(null)
    }

    fun abrir(ligaId: String?) {
        carregar {
            if (rede.isOnlineOneShot()) {
                try {
                    if (_ligas.value.isEmpty()) _ligas.value = servico.ligas().sortedBy { it.level }
                    val t = servico.classificacao(ligaId)
                    _tabela.value = t
                    _offline.value = false
                    guardarCache(t)
                    return@carregar
                } catch (e: Exception) {
                    Log.e("Classificacao", "API falhou, a usar a cópia guardada: ${e.message}")
                }
            }
            val copia = lerCache(ligaId) ?: throw Exception("Sem ligação e sem classificação guardada.")
            _tabela.value = copia
            _offline.value = true
        }
    }

    /** Guarda a última classificação vista no Firestore, para consultar sem rede. */
    private suspend fun guardarCache(t: StandingsDto) {
        val liga = t.league ?: return
        try {
            firestore.collection(COLECAO).document(liga.id)
                .set(mapOf("json" to gson.toJson(t), "ordem" to liga.level))
                .await()
        } catch (e: Exception) {
            Log.e("Classificacao", "Não foi possível guardar a cópia: ${e.message}")
        }
    }

    private suspend fun lerCache(ligaId: String?): StandingsDto? = try {
        val colecao = firestore.collection(COLECAO)
        val doc = if (ligaId != null) {
            colecao.document(ligaId).get().await()
        } else {
            colecao.orderBy("ordem").limit(1).get().await().documents.firstOrNull()
        }
        doc?.getString("json")?.let { gson.fromJson(it, StandingsDto::class.java) }
    } catch (e: Exception) {
        null
    }

    private companion object {
        const val COLECAO = "classificacao_cache"
    }
}

@Composable
fun ClassificacaoScreen(
    navHostController: NavHostController,
    viewModel: ClassificacaoViewModel = hiltViewModel()
) {
    val aCarregar by viewModel.aCarregar.collectAsStateWithLifecycle()
    val erro by viewModel.erroCarga.collectAsStateWithLifecycle()
    val ligas by viewModel.ligas.collectAsStateWithLifecycle()
    val tabela by viewModel.tabela.collectAsStateWithLifecycle()
    val offline by viewModel.offline.collectAsStateWithLifecycle()

    LoadingPage(
        isLoading = aCarregar && tabela == null,
        errorMsg = if (tabela == null) erro else null,
        retry = { viewModel.abrir(tabela?.league?.id) },
        content = {
            Column(
                modifier = Modifier
                    .fillMaxSize()
                    .verticalScroll(rememberScrollState())
                    .padding(16.dp)
            ) {
                OfflineBanner(isVisible = offline, text = "Sem ligação: a mostrar a última classificação guardada.")
                val t = tabela
                val ligaAtual = t?.league
                if (ligas.isNotEmpty() && ligaAtual != null) {
                    SelectBox(
                        list = ligas,
                        selectedValue = ligas.firstOrNull { it.id == ligaAtual.id } ?: ligas.first(),
                        onSelectItem = { viewModel.abrir(it.id) },
                        itemToString = { "${it.name} (escalão ${it.level})" },
                        modifier = Modifier.fillMaxWidth()
                    )
                }
                if (t != null) {
                    ConteudoClassificacao(
                        tabela = t,
                        aoAbrirEquipa = { id ->
                            navHostController.navigate("${Routes.TeamRoutes.TEAM_PROFILE.route}/$id") {
                                launchSingleTop = true
                            }
                        }
                    )
                }
            }
        }
    )
}

@Composable
private fun ConteudoClassificacao(tabela: StandingsDto, aoAbrirEquipa: (String) -> Unit) {
    val liga = tabela.league
    val epoca = tabela.season
    Spacer(Modifier.height(8.dp))
    Text(liga?.name ?: "Classificação", style = MaterialTheme.typography.headlineSmall, fontWeight = FontWeight.Bold)
    Text(
        text = if (epoca != null) "Época ${epoca.name} · ${Textos.estadoEpoca(epoca.status)}" else "Ainda não há época nesta liga",
        style = MaterialTheme.typography.bodyMedium,
        color = MaterialTheme.colorScheme.onSurfaceVariant
    )
    if (liga != null) {
        Text(
            "Vitória 3 pontos, empate 1, derrota 0 · sobem ${liga.promotionSpots}, descem ${liga.relegationSpots}",
            style = MaterialTheme.typography.bodySmall,
            color = MaterialTheme.colorScheme.onSurfaceVariant
        )
    }
    Spacer(Modifier.height(12.dp))

    val linhas = tabela.rows.orEmpty()
    if (linhas.isEmpty()) {
        Text("Ainda não há equipas inscritas nesta época.")
        return
    }

    Row(modifier = Modifier.horizontalScroll(rememberScrollState())) {
        Column {
            Cabecalho()
            HorizontalDivider()
            linhas.forEach { r ->
                LinhaTabela(r, aoAbrirEquipa)
                HorizontalDivider(color = MaterialTheme.colorScheme.outlineVariant)
            }
        }
    }
    Spacer(Modifier.height(12.dp))
    Legenda()
}

private val LARG_POS = 28.dp
private val LARG_EQUIPA = 130.dp
private val LARG_NUM = 34.dp
private val LARG_FORMA = 100.dp

@Composable
private fun Cabecalho() {
    Row(modifier = Modifier.padding(vertical = 6.dp), verticalAlignment = Alignment.CenterVertically) {
        Celula("#", LARG_POS, negrito = true)
        Celula("Equipa", LARG_EQUIPA, negrito = true, alinhar = TextAlign.Start)
        listOf("PD", "V", "E", "D", "GM", "GS", "DG", "P").forEach { Celula(it, LARG_NUM, negrito = true) }
        Celula("Forma", LARG_FORMA, negrito = true)
    }
}

@Composable
private fun LinhaTabela(r: StandingRowDto, aoAbrirEquipa: (String) -> Unit) {
    val corZona = when (r.zone) {
        "PROMOTION" -> CoresCompeticao.SUBIDA
        "RELEGATION" -> CoresCompeticao.DESCIDA
        else -> Color.Transparent
    }
    Row(
        modifier = Modifier
            .clickable { aoAbrirEquipa(r.teamId) }
            .padding(vertical = 8.dp),
        verticalAlignment = Alignment.CenterVertically
    ) {
        Box(
            modifier = Modifier
                .width(4.dp)
                .height(20.dp)
                .background(corZona)
        )
        Celula(r.position.toString(), LARG_POS - 4.dp)
        Celula(r.teamName, LARG_EQUIPA, alinhar = TextAlign.Start)
        Celula(r.played.toString(), LARG_NUM)
        Celula(r.won.toString(), LARG_NUM)
        Celula(r.drawn.toString(), LARG_NUM)
        Celula(r.lost.toString(), LARG_NUM)
        Celula(r.goalsFor.toString(), LARG_NUM)
        Celula(r.goalsAgainst.toString(), LARG_NUM)
        Celula(Textos.comSinal(r.goalDifference), LARG_NUM)
        Celula(r.points.toString(), LARG_NUM, negrito = true)
        Box(modifier = Modifier.width(LARG_FORMA), contentAlignment = Alignment.Center) {
            FormaIcones(r.form.orEmpty())
        }
    }
}

@Composable
private fun Celula(texto: String, largura: Dp, negrito: Boolean = false, alinhar: TextAlign = TextAlign.Center) {
    Text(
        text = texto,
        modifier = Modifier.width(largura),
        textAlign = alinhar,
        fontWeight = if (negrito) FontWeight.Bold else FontWeight.Normal,
        style = MaterialTheme.typography.bodySmall,
        maxLines = 1,
        overflow = TextOverflow.Ellipsis
    )
}

@Composable
private fun Legenda() {
    Text(
        "PD jogos disputados · V vitórias · E empates · D derrotas · GM golos marcados · " +
            "GS golos sofridos · DG diferença de golos · P pontos",
        style = MaterialTheme.typography.bodySmall,
        color = MaterialTheme.colorScheme.onSurfaceVariant
    )
    Row(horizontalArrangement = Arrangement.spacedBy(12.dp), modifier = Modifier.padding(top = 6.dp)) {
        Row(verticalAlignment = Alignment.CenterVertically) {
            Bolinha(CoresCompeticao.SUBIDA, 10); Spacer(Modifier.width(4.dp)); Text("Subida", style = MaterialTheme.typography.bodySmall)
        }
        Row(verticalAlignment = Alignment.CenterVertically) {
            Bolinha(CoresCompeticao.DESCIDA, 10); Spacer(Modifier.width(4.dp)); Text("Descida", style = MaterialTheme.typography.bodySmall)
        }
    }
}
