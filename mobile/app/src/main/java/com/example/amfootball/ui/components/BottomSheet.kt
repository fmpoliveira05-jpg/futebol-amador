package com.example.amfootball.ui.components

import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.ModalBottomSheet
import androidx.compose.runtime.Composable

/**
 * Wrapper personalizado para o componente [ModalBottomSheet] do Material Design 3.
 *
 * Este componente encapsula a lógica de apresentação de uma folha modal (que desliza do fundo do ecrã),
 * isolando a necessidade de anotações experimentais (`@OptIn`) no resto da aplicação.
 *
 * É utilizado principalmente para exibir menus de contexto secundários (como o menu "Mais Opções" da BottomBar)
 * ou formulários rápidos, sem que o utilizador perca o contexto do ecrã principal por trás.
 *
 * @param onDismiss Callback executado quando a folha é dispensada.
 * Disparado quando o utilizador clica na área escura (scrim), arrasta a folha para baixo ou pressiona o botão Voltar.
 * **Importante:** A implementação deve atualizar o estado que controla a visibilidade deste componente para `false`.
 *
 * @param content O conteúdo Composable a ser renderizado dentro do corpo da folha modal (Slot API).
 */
@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun AppModalBottomSheet(
    onDismiss: () -> Unit,
    content: @Composable () -> Unit
) {
    ModalBottomSheet(
        onDismissRequest = onDismiss
    ) {
        content()
    }
}
