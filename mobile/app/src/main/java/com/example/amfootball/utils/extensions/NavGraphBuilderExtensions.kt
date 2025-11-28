package com.example.amfootball.utils.extensions

import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.navigation.NamedNavArgument
import androidx.navigation.NavBackStackEntry
import androidx.navigation.NavGraphBuilder
import androidx.navigation.NavHostController
import androidx.navigation.compose.composable
import com.example.amfootball.data.local.SessionManager
import com.example.amfootball.navigation.objects.Routes

/**
 * Extensão personalizada do NavGraphBuilder para criar Rotas Protegidas.
 *
 * Esta função funciona exatamente como o 'composable()' padrão, mas adiciona
 * uma camada de verificação de segurança antes de renderizar o ecrã.
 *
 * @param route A string da rota (URL) para este destino (ex: "profile").
 * @param navController O controlador necessário para redirecionar o user se não estiver logado.
 * @param sessionManager A classe que guarda/verifica o token de autenticação.
 * @param arguments Lista de argumentos opcionais da rota (ex: IDs). Padrão é vazio.
 * @param content O Ecrã (@Composable) que deve ser mostrado se o user estiver autorizado.
 */
fun NavGraphBuilder.composableProtected(
    route: String,
    navController: NavHostController,
    sessionManager: SessionManager,
    arguments: List<NamedNavArgument> = emptyList(),
    content: @Composable (NavBackStackEntry) -> Unit
) {
    composable(route = route, arguments = arguments) { backStackEntry ->
        val token = sessionManager.getAuthToken()

        val isAuthenticated = token != null
        if (isAuthenticated) {
            content(backStackEntry)
        } else {
            LaunchedEffect(Unit) {
                navController.navigate(Routes.UserRoutes.LOGIN.route) {
                    popUpTo(route) { inclusive = true }
                }
            }
        }
    }
}

/**
 * Extensão do [NavGraphBuilder] que define uma rota de navegação acessível apenas a utilizadores **não** autenticados (Visitantes).
 *
 * Esta função é ideal para ecrãs como Login ou Registo, onde não faz sentido um utilizador já logado aceder.
 *
 * Comportamento:
 * - **Não Autenticado:** O ecrã definido em [content] é renderizado (ex: Mostra o formulário de Login).
 * - **Autenticado:** Se o utilizador já possuir um token válido, é redirecionado automaticamente para a Homepage ([Routes.GeralRoutes.HOMEPAGE]),
 * impedindo o acesso desnecessário ao ecrã de autenticação.
 *
 * @param route A string única que identifica a rota (URL) no grafo de navegação.
 * @param navController O controlador de navegação usado para redirecionar o utilizador para a Home se já estiver logado.
 * @param sessionManager O gestor de sessão responsável por verificar se já existe um token ativo.
 * @param arguments Lista de argumentos opcionais para a rota. O padrão é uma lista vazia.
 * @param content O conteúdo Composable (Ecrã) a ser exibido apenas se o utilizador for um visitante (não logado).
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
            LaunchedEffect(Unit) {
                navController.navigate(Routes.GeralRoutes.HOMEPAGE.route) {
                    popUpTo(route) { inclusive = true }
                }
            }
        }
    }
}