package com.example.amfootball.ui.components.notification

import android.content.Context
import android.widget.Toast
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.ui.platform.LocalContext
import com.example.amfootball.R


/**
 * Componente invisível responsável por observar uma mensagem e exibir um Toast.
 * Após exibir, chama a função [onToastShown] para limpar o estado.
 *
 * @param toastMessage A mensagem a ser exibida (pode ser null).
 * @param onToastShown Função de callback para limpar a mensagem no ViewModel.
 */
@Composable
fun ToastHandler(
    toastMessage: String?,
    onToastShown: () -> Unit
) {
    val context = LocalContext.current

    LaunchedEffect(toastMessage) {
        if (toastMessage != null) {
            Toast.makeText(context, toastMessage, Toast.LENGTH_LONG).show()
            onToastShown()
        }
    }
}
/**
 * Exibe uma mensagem flutuante (Toast) para informar o utilizador.
 *
 * Por defeito, exibe uma mensagem de "Sem conexão à internet", mas pode ser personalizada
 * para mostrar qualquer outro aviso rápido.
 *
 * @param context O contexto da aplicação ou atividade necessário para exibir o Toast.
 * Em Jetpack Compose, geralmente obtido via `LocalContext.current`.
 * @param text A mensagem de texto a ser exibida.
 * Padrão: [R.string.toast_offline] ("Sem conexão à internet...").
 */
fun showOfflineToast(
    context: Context,
    text: String = context.getString(R.string.toast_offline)
) {
    Toast.makeText(
        context,
        text,
        Toast.LENGTH_SHORT
    ).show()
}