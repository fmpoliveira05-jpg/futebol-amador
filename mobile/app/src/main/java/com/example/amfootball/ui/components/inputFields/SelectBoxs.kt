package com.example.amfootball.ui.components.inputFields

import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.DropdownMenuItem
import androidx.compose.material3.ExposedDropdownMenuBox
import androidx.compose.material3.ExposedDropdownMenuDefaults
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.material3.TextField
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.unit.dp
import com.example.amfootball.R

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun<T> SelectBox(
    list: List<T>,
    selectedValue: T,
    onSelectItem: (T) -> Unit,
    itemToString: @Composable (T) -> String,
    isError: Boolean = false,
    modifier: Modifier = Modifier
) {

    var isExpanded by remember { mutableStateOf(false) }

    ExposedDropdownMenuBox(
        expanded = isExpanded,
        onExpandedChange = { isExpanded = !isExpanded },
        modifier = modifier
    ) {
        TextField(
            value = itemToString(selectedValue),
            onValueChange = { },
            readOnly = true,
            trailingIcon = {
                ExposedDropdownMenuDefaults.TrailingIcon(expanded = isExpanded)
            },
            modifier = Modifier.fillMaxWidth()
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
            isError = isError,
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