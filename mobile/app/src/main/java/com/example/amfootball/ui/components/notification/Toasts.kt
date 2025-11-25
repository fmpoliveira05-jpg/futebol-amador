package com.example.amfootball.ui.components.notification

import android.content.Context
import android.widget.Toast
import com.example.amfootball.R

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