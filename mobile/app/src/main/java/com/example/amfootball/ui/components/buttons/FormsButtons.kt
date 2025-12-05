package com.example.amfootball.ui.components.buttons

import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.Login
import androidx.compose.material.icons.filled.Cancel
import androidx.compose.material.icons.filled.Save
import androidx.compose.material3.Button
import androidx.compose.material3.ButtonDefaults
import androidx.compose.material3.Icon
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.vector.ImageVector
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.unit.dp
import com.example.amfootball.R

/**
 * Um componente Composable que renderiza um botão de ação primária (submit) para formulários.
 *
 * Este botão segue as diretrizes do Material Design 3, ocupando a largura máxima disponível
 * e apresentando uma altura standard de acessibilidade (56dp).
 * Utiliza as cores primárias do tema (`MaterialTheme.colorScheme.primary`).
 *
 * @param onClick A função lambda a ser executada no evento de clique (ex: validar e enviar dados).
 * @param imageButton O ícone [ImageVector] a exibir à esquerda do texto. Padrão: [Icons.Default.Save].
 * @param text O rótulo do botão. Padrão: Recurso de string `submit_button`.
 * @param contentDescription Texto para leitores de ecrã descreverem o ícone.
 */
@Composable
fun SubmitFormButton(
    onClick: () -> Unit,
    imageButton: ImageVector = Icons.Default.Save,
    text: String = stringResource(id = R.string.submit_button),
    contentDescription: String = stringResource(id = R.string.submit_button_description)
) {
    Button(
        onClick = onClick,
        modifier = Modifier
            .fillMaxWidth()
            .height(56.dp)
    ) {
        Icon(
            imageVector = imageButton,
            contentDescription = contentDescription
        )
        Spacer(Modifier.width(8.dp))
        Text(text = text)
    }
}

/**
 * Um botão de ação destrutiva ou negativa, estilizado com as cores de erro do tema.
 *
 * Este componente é visualmente distinto para alertar o utilizador sobre ações sensíveis
 * como "Cancelar", "Apagar" ou "Rejeitar". Utiliza [MaterialTheme.colorScheme.error] para o fundo
 * e [MaterialTheme.colorScheme.onError] para o texto/ícone.
 *
 * @param onClick Lambda executado quando o botão é clicado.
 * @param imageButton Ícone a ser exibido (padrão: [Icons.Default.Cancel]).
 * @param text O texto a ser exibido no botão.
 * @param contentDescription Descrição para acessibilidade (TalkBack).
 * @param modifier Modificador base para o botão.
 * @param textFieldModifier Modificador adicional útil para alinhar o botão com campos de texto (ex: padding ou width específico).
 */
@Composable
fun SubmitCancelButton(
    onClick: () -> Unit,
    imageButton: ImageVector = Icons.Default.Cancel,
    text: String,
    contentDescription: String,
    modifier: Modifier = Modifier,
    textFieldModifier: Modifier = Modifier
) {
    Button(
        onClick = onClick,
        modifier = modifier
            .fillMaxWidth()
            .height(56.dp)
            .then(textFieldModifier),
        colors = ButtonDefaults.buttonColors(
            containerColor = MaterialTheme.colorScheme.error,
            contentColor = MaterialTheme.colorScheme.onError
        )
    ) {
        Icon(
            imageVector = imageButton,
            contentDescription = contentDescription
        )
        Spacer(Modifier.width(8.dp))
        Text(text = text)
    }
}

/**
 * Botão especializado para o ecrã de Login/Autenticação.
 *
 * Apresenta um estilo visual distinto com cantos mais arredondados (12dp) e elevação (sombra),
 * destacando-se como a ação principal ("Call to Action") na página de entrada.
 * Inclui automaticamente o ícone de login espelhado (AutoMirrored) para suporte RTL.
 *
 * @param onClick Lambda disparado ao clicar em "Entrar".
 * @param modifier Modificador base do layout.
 * @param textFieldModifier Modificador extra para consistência de layout com os inputs de email/password.
 */
@Composable
fun LoginButton(
    onClick: () -> Unit,
    modifier: Modifier = Modifier,
    textFieldModifier: Modifier = Modifier
) {
    Button(
        onClick = onClick,
        modifier = modifier
            .fillMaxWidth()
            .height(54.dp)
            .then(textFieldModifier),
        shape = RoundedCornerShape(12.dp),
        colors = ButtonDefaults.buttonColors(
            containerColor = MaterialTheme.colorScheme.primary,
            contentColor = MaterialTheme.colorScheme.onPrimary
        ),
        elevation = ButtonDefaults.buttonElevation(
            defaultElevation = 6.dp,
            pressedElevation = 2.dp
        )
    ) {
        Text(text = stringResource(R.string.button_login))

        Spacer(modifier = Modifier.width(8.dp))

        Icon(
            imageVector = Icons.AutoMirrored.Filled.Login,
            contentDescription = null,
            modifier = Modifier.height(20.dp)
        )
    }
}