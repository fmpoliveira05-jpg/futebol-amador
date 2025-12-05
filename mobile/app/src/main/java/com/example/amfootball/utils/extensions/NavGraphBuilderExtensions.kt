package com.example.amfootball.utils.extensions

import android.net.Uri
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.navigation.NamedNavArgument
import androidx.navigation.NavBackStackEntry
import androidx.navigation.NavGraphBuilder
import androidx.navigation.NavHostController
import androidx.navigation.compose.composable
import com.example.amfootball.data.dtos.player.PlayerProfileDto
import com.example.amfootball.data.enums.UserRole
import com.example.amfootball.data.local.SessionManager
import com.example.amfootball.navigation.objects.Routes

//Rotas protegidas
/**
 * Define uma rota protegida básica que exige apenas **Autenticação**.
 *
 * Qualquer utilizador com um token válido (seja ele jogador sem equipa, membro ou admin)
 * pode aceder a esta rota (ex: Ecrã de Perfil, Definições Gerais).
 *
 * @param route A string única que identifica a rota no grafo de navegação (ex: "profile").
 * @param navController O controlador de navegação, usado para redirecionar o utilizador caso não esteja autenticado.
 * @param sessionManager O gestor de sessão injetado, usado para verificar a existência do token de autenticação.
 * @param arguments Lista de argumentos opcionais que a rota pode receber (ex: IDs). Predefinição: lista vazia.
 * @param content O conteúdo Composable (o ecrã) a ser renderizado se o utilizador estiver autenticado.
 *
 * @see composableGenericGuard Para detalhes sobre a lógica de redirecionamento.
 */
fun NavGraphBuilder.composableProtected(
    route: String,
    navController: NavHostController,
    sessionManager: SessionManager,
    arguments: List<NamedNavArgument> = emptyList(),
    content: @Composable (NavBackStackEntry) -> Unit
) {
    composableGenericGuard(
        route = route,
        navController = navController,
        sessionManager = sessionManager,
        arguments = arguments,
        validator = { true }, //Aceita tipo de user
        content = content
    )
}

/**
 * Define uma rota protegida exclusiva para utilizadores **sem equipa**.
 *
 * Esta rota é utilizada para fluxos de *Onboarding*, como "Criar Equipa" ou "Procurar Equipa".
 * Se um utilizador que já tem equipa tentar aceder, será considerado "Não Autorizado" para esta vista específica.
 *
 * **Role Exigido:** [UserRole.PLAYER_WITHOUT_TEAM]
 *
 * @param route A string única que identifica a rota no grafo de navegação.
 * @param navController O controlador de navegação para gerir redirecionamentos (Login ou Home).
 * @param sessionManager O gestor de sessão para verificar o token e o [UserRole] do perfil.
 * @param arguments Lista de argumentos opcionais para a rota. Predefinição: lista vazia.
 * @param content O conteúdo Composable a ser renderizado se o utilizador cumprir os requisitos.
 */
fun NavGraphBuilder.composableProtectedPlayerWithouTeam(
    route: String,
    navController: NavHostController,
    sessionManager: SessionManager,
    arguments: List<NamedNavArgument> = emptyList(),
    content: @Composable (NavBackStackEntry) -> Unit
) {
    composableGenericGuard(
        route = route,
        navController = navController,
        sessionManager = sessionManager,
        arguments = arguments,
        validator = { profile ->
            profile?.role == UserRole.PLAYER_WITHOUT_TEAM
        },
        content = content
    )
}

/**
 * Define uma rota protegida para membros de uma equipa (Jogadores ou Admins).
 *
 * Esta rota destina-se a áreas comuns da equipa, como Chat, Calendário ou Lista de Membros,
 * onde tanto o capitão como os jogadores têm acesso de leitura/escrita.
 *
 * **Roles Aceites:** [UserRole.MEMBER_TEAM] ou [UserRole.ADMIN_TEAM].
 *
 * @param route A string única que identifica a rota no grafo de navegação.
 * @param navController O controlador de navegação para gerir redirecionamentos.
 * @param sessionManager O gestor de sessão para verificar o token e o [UserRole].
 * @param arguments Lista de argumentos opcionais para a rota. Predefinição: lista vazia.
 * @param content O conteúdo Composable a ser renderizado se o utilizador pertencer a uma equipa.
 */
fun NavGraphBuilder.composableProtectedMemberTeam(
    route: String,
    navController: NavHostController,
    sessionManager: SessionManager,
    arguments: List<NamedNavArgument> = emptyList(),
    content: @Composable (NavBackStackEntry) -> Unit
) {
    composableGenericGuard(
        route = route,
        navController = navController,
        sessionManager = sessionManager,
        arguments = arguments,
        validator = { profile ->
            profile?.role == UserRole.MEMBER_TEAM || profile?.role == UserRole.ADMIN_TEAM
        },
        content = content
    )
}

/**
 * Define uma rota protegida exclusiva para **Administradores** de equipa.
 *
 * Esta rota é estritamente reservada para operações de gestão, como "Editar Equipa",
 * "Gerir Pedidos de Adesão" ou "Expulsar Membros". Jogadores normais serão redirecionados
 * para a Home se tentarem aceder.
 *
 * **Role Exigido:** [UserRole.ADMIN_TEAM]
 *
 * @param route A string única que identifica a rota no grafo de navegação.
 * @param navController O controlador de navegação para gerir redirecionamentos.
 * @param sessionManager O gestor de sessão para verificar o token e validar se o utilizador é Admin.
 * @param arguments Lista de argumentos opcionais para a rota. Predefinição: lista vazia.
 * @param content O conteúdo Composable a ser renderizado apenas para administradores.
 */
fun NavGraphBuilder.composableProtectedAdminTeam(
    route: String,
    navController: NavHostController,
    sessionManager: SessionManager,
    arguments: List<NamedNavArgument> = emptyList(),
    content: @Composable (NavBackStackEntry) -> Unit
) {
    composableGenericGuard(
        route = route,
        navController = navController,
        sessionManager = sessionManager,
        arguments = arguments,
        validator = { profile ->
            profile?.role == UserRole.ADMIN_TEAM
        },
        content = content
    )
}

//Rotas apenas para não autenticados
/**
 * Define uma rota acessível apenas a **visitantes** (Utilizadores Não Autenticados).
 *
 * É utilizada para ecrãs de entrada como **Login** ou **Registo**.
 *
 * **Comportamento:**
 * - **Visitante:** Vê o conteúdo normalmente.
 * - **Utilizador Logado:** É impedido de ver o ecrã e redirecionado automaticamente para a aplicação.
 *
 * **Lógica de Redirecionamento (Prioridade):**
 * 1. **Argumento `redirect`:** Se a URL tiver `?redirect=/rota/anterior`, o utilizador é enviado de volta para lá.
 * 2. **Homepage:** Caso contrário, segue para [Routes.GeralRoutes.HOMEPAGE].
 *
 * @param route A string da rota (ex: "Login?redirect={redirect}").
 * @param navController O controlador usado para navegar para o destino correto após autenticação detetada.
 * @param sessionManager O gestor de sessão para monitorizar o estado do login (token).
 * @param arguments Lista de argumentos, deve incluir o argumento "redirect" se a rota o suportar.
 * @param content O conteúdo Composable a ser exibido apenas para visitantes.
 */
fun NavGraphBuilder.composableNotProtectedRoute(
    route: String,
    navController: NavHostController,
    sessionManager: SessionManager,
    arguments: List<NamedNavArgument> = emptyList(),
    content: @Composable (NavBackStackEntry) -> Unit
) {
    composable(route = route, arguments = arguments) { backStackEntry ->
        val token = sessionManager.getAuthToken()

        val isAuthenticated = token != null
        if (!isAuthenticated) {
            content(backStackEntry)
        } else {

            val redirectParam = backStackEntry.arguments?.getString("redirect")

            LaunchedEffect(Unit) {
                if (redirectParam != null) {
                    val decodedRoute = Uri.decode(redirectParam)
                    navController.navigate(decodedRoute) {
                        popUpTo(route) { inclusive = true }
                    }
                } else {
                    navController.navigate(Routes.GeralRoutes.HOMEPAGE.route) {
                        popUpTo(route) { inclusive = true }
                    }
                }
            }
        }
    }
}

//Metodos privados, de suporte
/**
 * Função "Guarda" genérica que centraliza a lógica de controlo de acesso (RBAC - Role Based Access Control).
 *
 * Esta função privada evita a duplicação de código, gerindo os três estados possíveis de navegação:
 * 1. **Autorizado:** O conteúdo é exibido.
 * 2. **Não Autenticado:** Redirecionamento para Login (com parâmetro de retorno).
 * 3. **Role Inválido:** Redirecionamento para Home (prevenindo acesso não autorizado de utilizadores logados).
 *
 * @param route A rota a ser registada.
 * @param navController O controlador de navegação.
 * @param sessionManager O gestor de sessão.
 * @param arguments Argumentos da rota.
 * @param validator Uma função lambda (predicado) que recebe o [PlayerProfileDto] e retorna `true` se o utilizador tiver permissão.
 * @param content O conteúdo a renderizar em caso de sucesso.
 */
private fun NavGraphBuilder.composableGenericGuard(
    route: String,
    navController: NavHostController,
    sessionManager: SessionManager,
    arguments: List<NamedNavArgument>,
    validator: (PlayerProfileDto?) -> Boolean,
    content: @Composable (NavBackStackEntry) -> Unit
) {
    composable(route = route, arguments = arguments) { backStackEntry ->
        val token = sessionManager.getAuthToken()
        val userProfile = sessionManager.getUserProfile()
        val isAuthenticated = !token.isNullOrEmpty()
        val isAuthorized = isAuthenticated && validator(userProfile)

        when {
            isAuthorized -> {
                content(backStackEntry)
            }

            !isAuthenticated -> {
                LaunchedEffect(Unit) {
                    val encodedRedirect = Uri.encode(route)
                    val destination = "${Routes.UserRoutes.LOGIN.route}?redirect=$encodedRedirect"
                    navController.navigate(destination) {
                        popUpTo(route) { inclusive = true }
                    }
                }
            }

            else -> {
                LaunchedEffect(Unit) {
                    navController.navigate(Routes.GeralRoutes.HOMEPAGE.route) {
                        popUpTo(route) { inclusive = true }
                    }
                }
            }
        }
    }
}