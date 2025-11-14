package com.example.amfootball.ui.components.inputFields

import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.width
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Check
import androidx.compose.material.icons.filled.Close
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.Icon
import androidx.compose.material3.Switch
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun Switcher(value: Boolean = true,
             onCheckedChange: (Boolean) -> Unit,
             text: String,
             textChecked: String,
             textUnChecked: String) {
    Column {
        Row (
            verticalAlignment = Alignment.CenterVertically
        ){
            Text(text = text)

            Spacer(modifier = Modifier.width(8.dp))

            Switch(
                checked = value,
                onCheckedChange = { novoValor ->
                    onCheckedChange(novoValor)
                },
                //Permite mudar o icon de quando está ativo ou não
                thumbContent = {
                    Icon(
                        imageVector = if(value) {
                            Icons.Default.Check
                        } else {
                            Icons.Default.Close
                        },
                        contentDescription = if(value) {
                            textChecked
                        } else {
                            textUnChecked
                        }

                    )
                }
            )
        }
    }
}