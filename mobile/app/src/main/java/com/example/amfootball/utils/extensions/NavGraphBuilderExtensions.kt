package com.example.amfootball.utils.extensions

import android.net.Uri
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
 * Extensão do [NavGraphBuilder] que define uma rota **protegida** (Requer Autenticação).
 *
 * Esta função atua como um "Guarda" de segurança para ecrãs que exigem login (ex: Perfil, Definições).
 *
 * **Comportamento:**
 * - **Autenticado:** Se o utilizador tiver um token válido no [sessionManager], o conteúdo [content] é renderizado normalmente.
 * - **Não Autenticado:** O utilizador é redirecionado imediatamente para o ecrã de Login (`Routes.UserRoutes.LOGIN`).
 *
 * **Funcionalidade de Redirecionamento (Redirect Back):**
 * Antes de navegar para o login, esta função captura a rota atual (onde o utilizador tentou entrar),
 * codifica-a (URL Encode) e anexa-a como o parâmetro `redirect` na URL do Login.
 * Exemplo: Se tentar aceder a `team/edit`, navega para `Login?redirect=team%2Fedit`.
 *
 * @param route A string única que identifica a rota no grafo de navegação.
 * @param navController O controlador para gerir a navegação e expulsar o utilizador não autenticado.
 * @param sessionManager O gestor de sessão para verificar a existência do token.
 * @param arguments Lista de argumentos opcionais para a rota (padrão vazio).
 * @param content O conteúdo Composable a ser exibido se o utilizador estiver autenticado.
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
                val encodedRedirect = Uri.encode(route)
                val destination = "${Routes.UserRoutes.LOGIN.route}?redirect=$encodedRedirect"

                navController.navigate(destination) {
                    popUpTo(route) { inclusive = true }
                }
            }
        }
    }
}

/**
 * Extensão do [NavGraphBuilder] que define uma rota acessível apenas a **visitantes** (Não Autenticados).
 *
 * É utilizada para ecrãs como Login ou Registo. Esta função gere também o fluxo de navegação pós-login.
 *
 * **Comportamento:**
 * - **Não Autenticado (Visitante):** O conteúdo [content] é mostrado (ex: Formulário de Login).
 * - **Autenticado (Já Logado):** Se o utilizador já tem token (ou acabou de fazer login com sucesso),
 * esta função impede a visualização do ecrã e redireciona o utilizador para fora.
 *
 * **Lógica de Destino (Prioridade):**
 * Ao redirecionar um utilizador autenticado, a função verifica os argumentos da rota atual:
 * 1. **Com Redirecionamento:** Se existir um argumento `redirect` (ex: veio de uma rota protegida),
 * o utilizador é enviado de volta para essa rota original.
 * 2. **Sem Redirecionamento:** Se não houver argumento `redirect` (login espontâneo),
 * o utilizador é enviado para a Homepage ([Routes.GeralRoutes.HOMEPAGE]).
 *
 * @param route A string da rota (ex: "Login?redirect={redirect}").
 * @param navController O controlador usado para navegar para o destino correto após autenticação.
 * @param sessionManager O gestor de sessão para monitorizar o estado do login.
 * @param arguments Lista de argumentos, deve incluir o argumento "redirect" se a rota o suportar.
 * @param content O conteúdo Composable a ser exibido para visitantes.
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