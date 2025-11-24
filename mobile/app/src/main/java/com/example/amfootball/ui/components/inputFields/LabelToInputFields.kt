package com.example.amfootball.ui.components.inputFields

import androidx.compose.foundation.layout.padding
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp

/**
 * Um componente Composable que exibe um rótulo de texto estilizado,
 * frequentemente usado acima de campos de entrada em formulários.
 *
 * Ele utiliza a função [formatRequiredLabel] para adicionar um asterisco (*)
 * se o campo for obrigatório, garantindo a consistência visual.
 *
 * @param label O texto principal do rótulo (ex: "Nome do Usuário").
 * @param isRequired Booleano que indica se o campo associado ao rótulo é obrigatório.
 */
@Composable
fun Label(label: String,
          isRequired: Boolean) {
    val labelText = formatRequiredLabel(label = label, isRequired = isRequired)

    Text(
        text = labelText,
        style = MaterialTheme.typography.bodyMedium,
        modifier = Modifier
            .padding(top = 16.dp)
    )
}

/**
 * Formata a string do rótulo, adicionando um asterisco no final se o campo for obrigatório.
 *
 * É uma função pura (não-Composable) para realizar a lógica de formatação.
 *
 * @param label A string base do rótulo.
 * @param isRequired Booleano que indica se o asterisco deve ser anexado.
 * @return A string do rótulo formatada (ex: "Nome" ou "Nome *").
 */
fun formatRequiredLabel(label: String, isRequired: Boolean): String {
    var labelText = label

    if (isRequired) {
        labelText = "$label *"
    }

    return labelText
}