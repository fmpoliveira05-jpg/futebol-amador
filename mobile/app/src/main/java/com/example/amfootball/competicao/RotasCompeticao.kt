package com.example.amfootball.competicao

import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.SwapHoriz
import androidx.compose.material.icons.filled.Description
import androidx.compose.material.icons.filled.EmojiEvents
import androidx.compose.material.icons.filled.Inbox
import androidx.compose.material.icons.filled.SportsSoccer
import androidx.compose.material.icons.filled.Storefront
import androidx.compose.ui.graphics.vector.ImageVector
import androidx.navigation.NavGraphBuilder
import androidx.navigation.NavHostController
import androidx.navigation.NavType
import androidx.navigation.compose.composable
import androidx.navigation.navArgument
import com.example.amfootball.R
import com.example.amfootball.core.extensions.composableProtected
import com.example.amfootball.core.extensions.composableProtectedAdminTeam
import com.example.amfootball.core.extensions.composableProtectedMemberTeam
import com.example.amfootball.data.local.SessionManager
import com.example.amfootball.ui.navigation.objects.AppRouteInfo

/** Rotas dos ecrãs de competição. */
object RotasCompeticao {
    const val ARG_JOGO = "jogoId"
    const val LIGAS = "ligas"
    const val MERCADO = "mercado"
    const val TRANSFERENCIAS = "transferenciasEquipa"
    const val PROPOSTAS = "propostasJogador"
    const val ONZE = "onze"
    const val RELATORIO = "relatorioJogo"
}

/** Entradas de menu e títulos da barra superior. */
enum class RotaCompeticao(
    override val route: String,
    override val labelResId: Int,
    override val icon: ImageVector,
    override val contentDescription: Int,
    override val haveBackButton: Boolean = false
) : AppRouteInfo {
    LIGAS(RotasCompeticao.LIGAS, R.string.item_ligas, Icons.Default.EmojiEvents, R.string.item_ligas_descricao),
    MERCADO(RotasCompeticao.MERCADO, R.string.item_mercado, Icons.Default.Storefront, R.string.item_mercado_descricao),
    TRANSFERENCIAS(
        RotasCompeticao.TRANSFERENCIAS, R.string.item_transferencias,
        Icons.Default.SwapHoriz, R.string.item_transferencias_descricao
    ),
    PROPOSTAS(RotasCompeticao.PROPOSTAS, R.string.item_propostas, Icons.Default.Inbox, R.string.item_propostas_descricao),
    ONZE(RotasCompeticao.ONZE, R.string.item_onze, Icons.Default.SportsSoccer, R.string.item_onze, haveBackButton = true),
    RELATORIO(RotasCompeticao.RELATORIO, R.string.item_relatorio, Icons.Default.Description, R.string.item_relatorio, haveBackButton = true)
}

/** Regista os ecrãs de competição no grafo de navegação. */
fun NavGraphBuilder.ecrasCompeticao(nav: NavHostController, sessao: SessionManager) {
    composable(RotasCompeticao.LIGAS) { LigasScreen(navHostController = nav) }

    composable(
        route = "${RotasCompeticao.RELATORIO}/{${RotasCompeticao.ARG_JOGO}}",
        arguments = listOf(navArgument(RotasCompeticao.ARG_JOGO) { type = NavType.StringType })
    ) { RelatorioScreen() }

    composableProtectedMemberTeam(
        route = "${RotasCompeticao.ONZE}/{${RotasCompeticao.ARG_JOGO}}",
        arguments = listOf(navArgument(RotasCompeticao.ARG_JOGO) { type = NavType.StringType }),
        navController = nav,
        sessionManager = sessao,
        content = { OnzeScreen() }
    )

    composableProtectedAdminTeam(
        route = RotasCompeticao.MERCADO,
        navController = nav,
        sessionManager = sessao,
        content = { MercadoScreen(navHostController = nav) }
    )

    composableProtectedAdminTeam(
        route = RotasCompeticao.TRANSFERENCIAS,
        navController = nav,
        sessionManager = sessao,
        content = { TransferenciasEquipaScreen(navHostController = nav) }
    )

    composableProtected(
        route = RotasCompeticao.PROPOSTAS,
        navController = nav,
        sessionManager = sessao,
        content = { PropostasJogadorScreen(navHostController = nav) }
    )
}
