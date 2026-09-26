package com.example.amfootball.competicao

import androidx.compose.foundation.Canvas
import androidx.compose.foundation.background
import androidx.compose.foundation.border
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.BoxWithConstraints
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.aspectRatio
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.offset
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Check
import androidx.compose.material.icons.filled.Close
import androidx.compose.material.icons.filled.Remove
import androidx.compose.material3.Card
import androidx.compose.material3.Icon
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.geometry.Offset
import androidx.compose.ui.geometry.Size
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.drawscope.Stroke
import androidx.compose.ui.semantics.contentDescription
import androidx.compose.ui.semantics.semantics
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp

/** Cores das bolinhas do calendário e da forma. */
object CoresCompeticao {
    val VITORIA = Color(0xFF2E7D32)
    val EMPATE = Color(0xFF757575)
    val DERROTA = Color(0xFFC62828)
    val SUBIDA = Color(0xFF2E7D32)
    val DESCIDA = Color(0xFFC62828)
    val RELVA = Color(0xFF2E7D32)
    val RELVA_CLARA = Color(0xFF388E3C)

    fun marca(tipo: TipoMarca): Color = when (tipo) {
        TipoMarca.FERIADO -> Color(0xFF9E9E9E)
        TipoMarca.AMIGAVEL -> Color(0xFF1E88E5)
        TipoMarca.LIGA -> Color(0xFF8E24AA)
        TipoMarca.TERMINADO -> Color(0xFF43A047)
        TipoMarca.CANCELADO -> Color(0xFFE53935)
        TipoMarca.ADIADO -> Color(0xFFFDD835)
    }
}

/** Título de uma secção. */
@Composable
fun TituloSeccao(texto: String, modifier: Modifier = Modifier) {
    Text(
        text = texto,
        style = MaterialTheme.typography.titleMedium,
        fontWeight = FontWeight.Bold,
        modifier = modifier.padding(top = 16.dp, bottom = 8.dp)
    )
}

/** Linha "rótulo: valor" de uma ficha. */
@Composable
fun LinhaFicha(rotulo: String, valor: String?) {
    Row(
        modifier = Modifier
            .fillMaxWidth()
            .padding(vertical = 4.dp),
        horizontalArrangement = Arrangement.SpaceBetween
    ) {
        Text(rotulo, style = MaterialTheme.typography.bodyMedium, color = MaterialTheme.colorScheme.onSurfaceVariant)
        Text(
            valor?.takeIf { it.isNotBlank() } ?: "—",
            style = MaterialTheme.typography.bodyMedium,
            fontWeight = FontWeight.SemiBold,
            textAlign = TextAlign.End
        )
    }
}

/** Mensagem de sucesso ou de erro por baixo do cabeçalho de um ecrã. */
@Composable
fun AvisoCompeticao(texto: String?, erro: Boolean) {
    if (texto.isNullOrBlank()) return
    Text(
        text = texto,
        color = if (erro) MaterialTheme.colorScheme.error else CoresCompeticao.VITORIA,
        style = MaterialTheme.typography.bodyMedium,
        modifier = Modifier.padding(vertical = 8.dp)
    )
}

/** Últimos resultados só com ícones (✓ vitória, – empate, ✕ derrota). */
@Composable
fun FormaIcones(forma: List<String>) {
    Row(horizontalArrangement = Arrangement.spacedBy(2.dp)) {
        forma.forEach { r ->
            val (icone, cor, descricao) = when (r) {
                "V" -> Triple(Icons.Filled.Check, CoresCompeticao.VITORIA, "Vitória")
                "D" -> Triple(Icons.Filled.Close, CoresCompeticao.DERROTA, "Derrota")
                else -> Triple(Icons.Filled.Remove, CoresCompeticao.EMPATE, "Empate")
            }
            Box(
                modifier = Modifier
                    .size(16.dp)
                    .background(cor, CircleShape),
                contentAlignment = Alignment.Center
            ) {
                Icon(icone, contentDescription = descricao, tint = Color.White, modifier = Modifier.size(12.dp))
            }
        }
    }
}

/** Uma bolinha colorida (calendário e legenda). */
@Composable
fun Bolinha(cor: Color, tamanho: Int = 6) {
    Box(
        modifier = Modifier
            .size(tamanho.dp)
            .background(cor, CircleShape)
    )
}

/** Uma posição desenhada no campo. */
data class PontoCampo(
    val chave: Int,
    val x: Double,
    val y: Double,
    val sigla: String,
    val nome: String?
)

/**
 * Campo de futebol (meio-campo da equipa, baliza em baixo) com os jogadores nas posições da tática.
 * [x] e [y] vêm em percentagem, como na API.
 */
@Composable
fun CampoFutebol(
    pontos: List<PontoCampo>,
    modifier: Modifier = Modifier,
    aoTocar: ((Int) -> Unit)? = null,
    selecionado: Int? = null
) {
    BoxWithConstraints(
        modifier = modifier
            .fillMaxWidth()
            .aspectRatio(0.72f)
            .background(CoresCompeticao.RELVA, RoundedCornerShape(8.dp))
            .semantics { contentDescription = "Campo com o onze inicial" }
    ) {
        val largura = maxWidth
        val altura = maxHeight
        Canvas(modifier = Modifier.matchParentSize()) {
            val linha = Color.White.copy(alpha = 0.7f)
            val traco = Stroke(width = 2.dp.toPx())
            val margem = 8.dp.toPx()
            // faixas de relva
            val faixas = 8
            val alturaFaixa = size.height / faixas
            for (i in 0 until faixas step 2) {
                drawRect(CoresCompeticao.RELVA_CLARA, Offset(0f, i * alturaFaixa), Size(size.width, alturaFaixa))
            }
            drawRect(linha, Offset(margem, margem), Size(size.width - 2 * margem, size.height - 2 * margem), style = traco)
            // linha do meio-campo e círculo central (em cima)
            drawArc(
                linha, 0f, 180f, false,
                Offset(size.width / 2 - size.width * 0.15f, margem - size.width * 0.15f),
                Size(size.width * 0.3f, size.width * 0.3f), style = traco
            )
            // grande área e pequena área (em baixo)
            val gaL = size.width * 0.6f
            val gaA = size.height * 0.18f
            drawRect(linha, Offset((size.width - gaL) / 2, size.height - margem - gaA), Size(gaL, gaA), style = traco)
            val paL = size.width * 0.3f
            val paA = size.height * 0.07f
            drawRect(linha, Offset((size.width - paL) / 2, size.height - margem - paA), Size(paL, paA), style = traco)
        }

        pontos.forEach { p ->
            val cx = largura * (p.x.toFloat() / 100f)
            val cy = altura * (p.y.toFloat() / 100f)
            Column(
                modifier = Modifier
                    .offset(x = cx - 36.dp, y = cy - 18.dp)
                    .width(72.dp)
                    .let { m -> if (aoTocar != null) m.clickable { aoTocar(p.chave) } else m },
                horizontalAlignment = Alignment.CenterHorizontally
            ) {
                val ativo = selecionado == p.chave
                Box(
                    modifier = Modifier
                        .size(28.dp)
                        .background(if (p.nome != null) Color.White else Color.White.copy(alpha = 0.35f), CircleShape)
                        .border(if (ativo) 3.dp else 1.dp, if (ativo) Color.Yellow else Color.Black.copy(alpha = 0.4f), CircleShape),
                    contentAlignment = Alignment.Center
                ) {
                    Text(p.sigla, fontSize = 9.sp, fontWeight = FontWeight.Bold, color = Color.Black)
                }
                Text(
                    text = p.nome?.let { abreviar(it) } ?: "Escolher",
                    fontSize = 10.sp,
                    color = Color.White,
                    maxLines = 1,
                    overflow = TextOverflow.Ellipsis,
                    textAlign = TextAlign.Center,
                    modifier = Modifier
                        .background(Color.Black.copy(alpha = 0.45f), RoundedCornerShape(4.dp))
                        .padding(horizontal = 3.dp)
                )
            }
        }
    }
}

/** "Francisco Miguel Oliveira" → "F. Oliveira". */
fun abreviar(nome: String): String {
    val partes = nome.trim().split(Regex("\\s+"))
    return if (partes.size < 2) nome else "${partes.first().first()}. ${partes.last()}"
}

/** Pontos do campo a partir de um onze já guardado (relatório ou onze do adversário). */
fun pontosDoOnze(onze: LineupDto, taticas: List<FormationDto>): List<PontoCampo> {
    val tatica = taticas.firstOrNull { it.code == onze.formation }
    return onze.starters.orEmpty().map { s ->
        val slot = tatica?.slots?.firstOrNull { it.slot == s.slot }
        PontoCampo(
            chave = s.slot,
            x = slot?.x ?: 50.0,
            y = slot?.y ?: (10.0 + s.slot * 8),
            sigla = s.positionCode,
            nome = s.playerName
        )
    }
}

/** Cartão simples com margem interior. */
@Composable
fun CartaoCompeticao(modifier: Modifier = Modifier, conteudo: @Composable () -> Unit) {
    Card(modifier = modifier
        .fillMaxWidth()
        .padding(vertical = 4.dp)) {
        Column(modifier = Modifier.padding(12.dp)) { conteudo() }
    }
}

/** Troféus da equipa, um por tipo, com o número de vezes ("x3") e as épocas. */
@Composable
fun TitulosEquipa(titulos: List<TeamTitleDto>) {
    if (titulos.isEmpty()) return
    Column(modifier = Modifier.fillMaxWidth()) {
        TituloSeccao("Títulos")
        titulos.forEach { t ->
            CartaoCompeticao {
                Row(verticalAlignment = Alignment.CenterVertically) {
                    Text("🏆", fontSize = 28.sp)
                    Column(modifier = Modifier.padding(start = 12.dp)) {
                        Text("x${t.count}", style = MaterialTheme.typography.titleLarge, fontWeight = FontWeight.Bold)
                        Text(t.trophyName, style = MaterialTheme.typography.bodyMedium)
                        Text(
                            "${t.leagueName} · " + t.seasons.orEmpty().joinToString(", "),
                            style = MaterialTheme.typography.bodySmall,
                            color = MaterialTheme.colorScheme.onSurfaceVariant
                        )
                    }
                }
            }
        }
    }
}
