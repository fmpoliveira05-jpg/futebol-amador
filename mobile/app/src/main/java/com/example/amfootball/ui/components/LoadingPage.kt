package com.example.amfootball.ui.components

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.material3.Button
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.res.stringResource
import com.example.amfootball.R

/**
 * Componente de alto nível que gerencia a exibição condicional do conteúdo de uma página
 * com base no estado de carregamento e erros.
 *
 * Ele garante que apenas um de três estados seja exibido:
 * 1. [Loading] (se [isLoading] for true).
 * 2. [RetryButton] (se [errorMsg] não for null).
 * 3. [content] (se não houver carregamento nem erro).
 *
 * @param isLoading Booleano que indica se a operação de dados está em andamento.
 * @param errorMsg A mensagem de erro a ser exibida. Se não for null, o botão de tentar novamente é mostrado.
 * @param retry Callback a ser executado quando o usuário clica no botão "Tentar Novamente".
 * @param content O conteúdo real da tela a ser exibido em caso de sucesso (sem erro ou carregamento).
 */
@Composable
fun LoadingPage(
    isLoading: Boolean,
    errorMsg: String?,
    retry: () -> Unit,
    content: @Composable () -> Unit,
) {
    if (isLoading) {
        Loading()
    }
    else if (errorMsg != null) {
        RetryButton(
            errorMsg = errorMsg,
            retry = retry
        )
    }
    else {
        content()
    }
}

/**
 * Componente Composable que exibe um indicador de progresso circular.
 *
 * O indicador é centralizado vertical e horizontalmente para ocupar a tela inteira.
 */
@Composable
fun Loading() {
    Column(
        modifier = Modifier.fillMaxSize(),
        verticalArrangement = Arrangement.Center,
        horizontalAlignment = Alignment.CenterHorizontally
    ) {
        CircularProgressIndicator()
    }
}

/**
 * Componente que exibe uma mensagem de erro formatada e um botão "Tentar Novamente".
 *
 * O texto do erro é exibido na cor vermelha.
 *
 * @param errorMsg A mensagem de erro (String, assumida como não null pelo contexto de uso).
 * @param retry Callback a ser executado quando o botão "Tentar Novamente" é clicado.
 */
@Composable
fun RetryButton(
    errorMsg: String?,
    retry: () -> Unit
) {
    Column {
        Text(text = errorMsg!!, color = Color.Red)
        Button(onClick = retry) {
            Text(text = stringResource(id = R.string.button_try_again))
        }
    }
}