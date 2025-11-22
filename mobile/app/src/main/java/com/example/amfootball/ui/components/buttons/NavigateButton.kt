package com.example.amfootball.ui.components.buttons

import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.material3.Button
import androidx.compose.material3.ButtonDefaults
import androidx.compose.material3.Icon
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import androidx.compose.ui.graphics.vector.ImageVector
import androidx.compose.ui.text.style.TextAlign

/**
 * Um componente Composable que cria um botão de navegação com ícone e rótulo dispostos verticalmente.
 *
 * O botão principal é renderizado como um círculo (usando [CircleShape]) e exibe um ícone.
 * Abaixo do botão circular, é exibido um [Text] como rótulo.
 * Este padrão é comumente usado em barras de navegação ou painéis de acesso rápido.
 *
 * @param modifier Um [Modifier] opcional para configurar o layout da coluna que contém o botão e o rótulo.
 * @param icon O [ImageVector] do ícone a ser exibido dentro do botão circular.
 * @param label A string de texto a ser usada como rótulo do botão (exibida abaixo do ícone)
 * e também como a descrição de conteúdo (acessibilidade) do ícone.
 * @param onClick A função lambda a ser executada quando o botão circular for clicado.
 */
@Composable
fun NavigateButton (
    modifier: Modifier = Modifier,
    icon: ImageVector,
    label: String,
    onClick: () -> Unit
) {
    Column(
            modifier = modifier,
            horizontalAlignment = Alignment.CenterHorizontally
        ) {
    Button (onClick = onClick,
        shape = CircleShape,
        modifier = Modifier.height(56.dp)
    ){
        Icon(
            imageVector = icon,
            contentDescription = label,
            modifier = Modifier.size(ButtonDefaults.IconSize * 1.5f) // Ícone um pouco maior
        )
        }
        Spacer(Modifier.width(8.dp))
        Text(
            text = label,
            style = MaterialTheme.typography.bodySmall,
            textAlign = TextAlign.Center,
            modifier = Modifier.padding(top = 4.dp)
        )
    }
}