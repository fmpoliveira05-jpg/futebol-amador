package com.example.amfootball.ui.components.notification

import android.widget.Toast
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.ui.platform.LocalContext

/**
 * Componente invisível responsável por observar uma mensagem e exibir um Toast.
 * Após exibir, chama a função [onToastShown] para limpar o estado.
 *
 * @param toastMessage A mensagem a ser exibida (pode ser null).
 * @param onToastShown Função de callback para limpar a mensagem no ViewModel.
 */
@Composable
fun ToastHandler(
    toastMessage: Int?,
    onToastShown: () -> Unit
) {
    val context = LocalContext.current

    LaunchedEffect(toastMessage) {
        if (toastMessage != null) {
            Toast.makeText(
                context,
                context.getString(toastMessage),
                Toast.LENGTH_LONG
            ).show()
            onToastShown()
        }
    }
}