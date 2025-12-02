package com.example.amfootball.ui.components.buttons

import android.net.Uri
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
import androidx.compose.ui.platform.testTag
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.unit.dp
import com.example.amfootball.R
import com.example.amfootball.navigation.objects.Routes

/**
 * Um componente Composable que renderiza um botão de envio (submit) para formulários.
 *
 * Este botão segue o padrão de design Material 3, ocupa a largura máxima disponível
 * ([Modifier.fillMaxWidth]) e tem uma altura fixa de 56.dp. Por padrão, ele
 * exibe o ícone [Icons.Default.Save].
 *
 * @param onClick A função lambda a ser executada quando o botão for clicado, tipicamente
 * contendo a lógica de validação e envio do formulário.
 * @param imageButton O [ImageVector] opcional para customizar o ícone exibido no botão
 * (padrão: [Icons.Default.Save]).
 * @param text A string de texto a ser exibida no botão (padrão: [R.string.submit_button]).
 * @param contentDescription A descrição de conteúdo para acessibilidade do ícone (padrão:
 * [R.string.submit_button_description]).
 */
@Composable
fun SubmitFormButton(onClick: () -> Unit,
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
 * Um botão de ação destrutiva/negativa estilizado com as cores de erro do tema.
 *
 * Este componente é ideal para ações como "Cancelar", "Apagar" ou "Rejeitar".
 * Ocupa a largura total ([Modifier.fillMaxWidth]) e usa [MaterialTheme.colorScheme.error]
 * para o fundo.
 *
 * @param onClick Lambda executado quando o botão é clicado.
 * @param imageButton Ícone a ser exibido (padrão: [Icons.Default.Cancel]).
 * @param text O texto a ser exibido no botão.
 * @param contentDescription Descrição para acessibilidade (TalkBack).
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