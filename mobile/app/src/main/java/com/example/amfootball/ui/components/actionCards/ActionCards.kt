package com.example.amfootball.ui.components.actionCards

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.width
import androidx.compose.material3.Card
import androidx.compose.material3.CardDefaults
import androidx.compose.material3.Icon
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.vector.ImageVector
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp

/**
 * Um componente de cartão interativo (Card) utilizado para opções de navegação ou execução de ações.
 *
 * Visualmente, este componente apresenta um layout horizontal contendo:
 * 1. Um ícone de destaque à esquerda, colorido com a cor primária do tema.
 * 2. Uma coluna de texto à direita, contendo um Título e um Subtítulo.
 *
 * O cartão possui uma elevação padrão de 2.dp para se destacar do fundo e responde a cliques.
 *
 * @param title O texto principal do cartão, exibido em negrito (SemiBold).
 * @param subtitle O texto de apoio ou descrição, exibido numa cor mais suave (OnSurfaceVariant).
 * @param icon O [ImageVector] a ser exibido no lado esquerdo do cartão.
 * @param onClick A função lambda (callback) a ser executada quando o utilizador clica no cartão.
 * @param textFieldModifier Um [Modifier] adicional opcional para personalizar o layout do cartão
 * (ex: adicionar margens externas ou alterar a altura). O nome do parâmetro sugere "textField",
 * mas aplica-se ao contentor do [Card].
 */
@Composable
fun ActionCard(
    title: String,
    subtitle: String,
    icon: ImageVector,
    onClick: () -> Unit,
    textFieldModifier: Modifier = Modifier
) {
    Card(
        onClick = onClick,
        modifier = Modifier
            .fillMaxWidth()
            .then(textFieldModifier),
        elevation = CardDefaults.cardElevation(defaultElevation = 2.dp)
    ) {
        Row(
            modifier = Modifier
                .padding(16.dp)
                .fillMaxWidth(),
            verticalAlignment = Alignment.CenterVertically
        ) {
            Icon(
                imageVector = icon,
                contentDescription = null,
                tint = MaterialTheme.colorScheme.primary,
                modifier = Modifier.size(40.dp)
            )

            Spacer(modifier = Modifier.width(16.dp))

            Column {
                Text(
                    text = title,
                    style = MaterialTheme.typography.titleLarge,
                    fontWeight = FontWeight.SemiBold
                )
                Text(
                    text = subtitle,
                    style = MaterialTheme.typography.bodyMedium,
                    color = MaterialTheme.colorScheme.onSurfaceVariant
                )
            }
        }
    }
}

/**
 * Wrapper de layout para organizar os cartões de ação numa coluna com espaçamento padrão.
 */
@Composable
fun ActionCardSection(content: @Composable () -> Unit) {
    Column(
        verticalArrangement = Arrangement.spacedBy(12.dp),
        modifier = Modifier.fillMaxWidth()
    ) {
        content()
    }
}

/**
 * Cartão de ação com layout compacto (Ícone no topo, Texto em baixo).
 * Ideal para grelhas ou linhas com múltiplos botões onde o espaço horizontal é limitado.
 *
 * @param title O título a exibir no cartão.
 * @param icon O ícone vetorial ([ImageVector]) a exibir.
 * @param onClick A função lambda a executar quando o cartão é clicado.
 * @param contentDescription Texto descritivo do ícone para acessibilidade.
 * @param modifier Modificador para aplicar estilos (ex: peso na linha).
 */
@Composable
fun CompactActionCard(
    title: String,
    icon: ImageVector,
    onClick: () -> Unit,
    contentDescription: String,
    modifier: Modifier = Modifier
) {
    Card(
        onClick = onClick,
        modifier = modifier.height(100.dp),
        elevation = CardDefaults.cardElevation(defaultElevation = 2.dp),
        colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surfaceVariant)
    ) {
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(8.dp),
            verticalArrangement = Arrangement.Center,
            horizontalAlignment = Alignment.CenterHorizontally
        ) {
            Icon(
                imageVector = icon,
                contentDescription = contentDescription,
                tint = MaterialTheme.colorScheme.primary,
                modifier = Modifier.size(32.dp)
            )

            Spacer(modifier = Modifier.height(8.dp))

            Text(
                text = title,
                style = MaterialTheme.typography.titleSmall,
                fontWeight = FontWeight.SemiBold,
                textAlign = TextAlign.Center
            )
        }
    }
}

