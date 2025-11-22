package com.example.amfootball.ui.components.inputFields

import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.DropdownMenuItem
import androidx.compose.material3.ExposedDropdownMenuBox
import androidx.compose.material3.ExposedDropdownMenuDefaults
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.OutlinedTextFieldDefaults
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.unit.dp
import com.example.amfootball.R

/**
 * Um componente Composable genérico que implementa um Exposed Dropdown Menu (Select Box).
 *
 * Utiliza um [OutlinedTextField] para exibir o valor selecionado e um [ExposedDropdownMenu]
 * para mostrar a lista de opções. É responsável por gerenciar o estado de expansão do menu.
 *
 * @param T O tipo de dados dos itens na lista (ex: String, DataObject, Enum).
 * @param list A lista de itens disponíveis para seleção.
 * @param selectedValue O item atualmente selecionado que será exibido no campo de texto.
 * @param onSelectItem Callback executado quando um item da lista é clicado.
 * @param itemToString Uma função Composable que define como o item do tipo [T] deve ser convertido
 * em uma String para exibição (tanto no campo quanto nos itens do menu).
 * @param modifier Modificador para estilizar o [ExposedDropdownMenuBox].
 */
@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun<T> SelectBox(
    list: List<T>,
    selectedValue: T,
    onSelectItem: (T) -> Unit,
    itemToString: @Composable (T) -> String,
    modifier: Modifier = Modifier
) {
    var isExpanded by remember { mutableStateOf(false) }

    ExposedDropdownMenuBox(
        expanded = isExpanded,
        onExpandedChange = { isExpanded = !isExpanded },
        modifier = modifier
    ) {
        OutlinedTextField(
            value = itemToString(selectedValue),
            onValueChange = { },
            readOnly = true,
            enabled = false,
            trailingIcon = {
                ExposedDropdownMenuDefaults.TrailingIcon(expanded = isExpanded)
            },
            modifier = Modifier
                .fillMaxWidth()
                .menuAnchor(),
            colors = OutlinedTextFieldDefaults.colors(
                disabledTextColor = MaterialTheme.colorScheme.onSurface,
                disabledContainerColor = Color.Transparent, // Outlined é transparente
                disabledBorderColor = MaterialTheme.colorScheme.outline,
                disabledLabelColor = MaterialTheme.colorScheme.onSurfaceVariant,
                disabledTrailingIconColor = MaterialTheme.colorScheme.onSurfaceVariant
            )
        )

        ExposedDropdownMenu(
            expanded = isExpanded,
            onDismissRequest = { isExpanded = false}
        ) {
            list.forEach { item ->
                val textToShow = itemToString(item)

                DropdownMenuItem(
                    text = { Text(text = textToShow) },
                    onClick = {
                        onSelectItem(item)
                        isExpanded = false
                    },
                    contentPadding = ExposedDropdownMenuDefaults.ItemContentPadding
                )
            }
        }
    }
}

/**
 * Um componente Composable genérico que combina um rótulo ([Label]) e o seletor
 * [SelectBox] com suporte para exibir mensagens de erro abaixo.
 *
 * É ideal para uso em formulários onde é necessário indicar se o campo é obrigatório
 * ou se há um erro de validação.
 *
 * @param T O tipo de dados dos itens na lista.
 * @param label A string do rótulo a ser exibida acima do campo de seleção.
 * @param list A lista de itens disponíveis para seleção.
 * @param selectedValue O item atualmente selecionado.
 * @param onSelectItem Callback executado ao selecionar um item.
 * @param itemToString Função que converte o item do tipo [T] para String.
 * @param modifier Modificador para estilizar o [Column] que envolve o rótulo e o campo.
 * @param isRequired Booleano para indicar se o campo é obrigatório (adiciona '*' ao rótulo).
 * @param isError Booleano que indica se deve ser exibida a mensagem de erro.
 * @param errorMessage A mensagem de erro a ser exibida abaixo do campo se [isError] for true.
 */
@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun <T> LabelSelectBox(
    label: String,
    list: List<T>,
    selectedValue: T,
    onSelectItem: (T) -> Unit,
    itemToString: @Composable (T) -> String,
    modifier: Modifier = Modifier,
    isRequired: Boolean = false,
    isError: Boolean = false,
    errorMessage: String = stringResource(id = R.string.mandatory_field)
) {
    Column(modifier = modifier) {
        Label(label = label, isRequired = isRequired)

        SelectBox(
            list = list,
            selectedValue = selectedValue,
            onSelectItem = onSelectItem,
            itemToString = itemToString,
        )

        if (isError) {
            Text(
                text = errorMessage,
                color = MaterialTheme.colorScheme.error,
                style = MaterialTheme.typography.bodySmall,
                modifier = Modifier.padding(start = 16.dp, top = 4.dp)
            )
        }
    }
}