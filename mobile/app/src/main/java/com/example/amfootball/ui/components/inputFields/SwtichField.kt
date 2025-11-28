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

/**
 * Um componente Composable que encapsula o [Switch] do Material 3,
 * adicionando um rótulo de texto e ícones personalizados para os estados ON/OFF.
 *
 * O ícone no botão deslizante (thumb) muda para [Icons.Default.Check] quando ligado
 * e para [Icons.Default.Close] quando desligado.
 *
 * @param value O estado atual do Switch (true para Ligado/Checked, false para Desligado/Unchecked).
 * O valor padrão é true.
 * @param onCheckedChange Callback executado quando o estado do Switch é alterado pelo usuário.
 * Recebe o novo valor (Boolean).
 * @param text O rótulo de texto principal exibido ao lado do Switch.
 * @param textChecked A descrição de conteúdo (acessibilidade) usada quando o Switch está LIGADO (Check).
 * @param textUnChecked A descrição de conteúdo (acessibilidade) usada quando o Switch está DESLIGADO (Close).
 */
@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun Switcher(
    value: Boolean = true,
    onCheckedChange: (Boolean) -> Unit,
    text: String,
    textChecked: String,
    textUnChecked: String,
    enabled: Boolean = true
) {
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