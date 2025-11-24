package com.example.amfootball.ui.components.buttons

import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.width
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Save
import androidx.compose.material3.Button
import androidx.compose.material3.Icon
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.vector.ImageVector
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.unit.dp
import com.example.amfootball.R

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