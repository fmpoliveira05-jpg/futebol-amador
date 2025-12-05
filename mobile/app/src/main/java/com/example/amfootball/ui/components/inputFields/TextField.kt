package com.example.amfootball.ui.components.inputFields

import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Visibility
import androidx.compose.material.icons.filled.VisibilityOff
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.saveable.rememberSaveable
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.text.input.PasswordVisualTransformation
import androidx.compose.ui.text.input.VisualTransformation
import com.example.amfootball.R

/**
 * Wrapper para [LabelTextField] especificamente para inputs numéricos.
 *
 * Garante que o teclado seja numérico e converte a String de input para Int? antes de chamar o callback.
 * A validação de comprimento ([min] e [max]) é usada para restringir o valor numérico.
 *
 * @param label Texto do rótulo do campo.
 * @param value Valor inteiro atual (pode ser null).
 * @param onValueChange Callback chamado quando o valor muda. Retorna Int? ou null se a conversão falhar.
 * @param min Valor mínimo permitido para o número (padrão: 0).
 * @param max Valor máximo permitido para o número (padrão: [Int.MAX_VALUE]).
 * @param modifier Modificador para estilizar o componente.
 */
@Composable
fun NumberTextField(
    label: String,
    value: Int?,
    onValueChange: (Int?) -> Unit,
    min: Int = 0,
    max: Int = Int.MAX_VALUE,
    modifier: Modifier = Modifier
) {
    LabelTextField(
        label = label,
        value = value?.toString() ?: "",
        minLenght = min,
        maxLenght = max,
        onValueChange = { onValueChange(it.toIntOrNull()) },
        keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number),
        modifier = modifier
    )
}

/**
 * Combina o [Label] (rótulo) com o campo de texto [TextFieldOutline].
 *
 * É o ponto de entrada principal para inputs de texto genéricos em formulários.
 *
 * @param label O texto do rótulo exibido acima do campo.
 * @param value O valor de texto atual do campo.
 * @param modifier Modificador para estilizar a [Column].
 * @param isSingleLine Se o campo deve ser de linha única.
 * @param onValueChange Callback chamado quando o valor de texto muda.
 * @param isReadOnly Se o campo deve ser apenas de leitura.
 * @param isRequired Se o campo é obrigatório (afeta o rótulo).
 * @param isError Se o campo está em estado de erro.
 * @param errorMessage Mensagem de erro a ser exibida.
 * @param keyboardOptions Opções de teclado ([KeyboardType], ação de entrada, etc.).
 * @param minLenght Comprimento mínimo da string para validação.
 * @param maxLenght Comprimento máximo da string para validação.
 */
@Composable
fun LabelTextField(
    label: String,
    value: String?,
    modifier: Modifier = Modifier,
    textFieldModifier: Modifier = Modifier,
    isSingleLine: Boolean = true,
    onValueChange: (String) -> Unit = {},
    isReadOnly: Boolean = false,
    isRequired: Boolean = false,
    isError: Boolean = false,
    errorMessage: String? = null,
    keyboardOptions: KeyboardOptions = KeyboardOptions.Default,
    minLenght: Int = 0,
    maxLenght: Int = Int.MAX_VALUE
) {
    Column(
        modifier = modifier
    ) {
        Label(label = label, isRequired = isRequired)

        TextFieldOutline(
            label = label,
            value = value,
            isSingleLine = isSingleLine,
            onValueChange = onValueChange,
            isReadOnly = isReadOnly,
            isError = isError,
            errorMessage = errorMessage,
            keyboardOptions = keyboardOptions,
            minLenght = minLenght,
            maxLenght = maxLenght,
            textFieldModifier = textFieldModifier
        )
    }
}

/**
 * O componente de entrada de texto base ([OutlinedTextField]) com tratamento de lógica.
 *
 * Inclui:
 * 1. Tratamento de campos de [KeyboardType.Password] com ícone de visibilidade (olho).
 * 2. Validação de [minLenght] e [maxLenght] em cada mudança de valor (via [isValueValid]).
 * 3. Exibição de texto de erro ([supportingText]).
 *
 * @param label O texto do rótulo base (usado para o placeholder e para o cálculo do asterisco de obrigatório).
 * @param value O valor de texto atual.
 * @param modifier Modificador para estilizar a [Column].
 * @param isSingleLine Se o campo deve ser de linha única.
 * @param onValueChange Callback chamado quando o valor de texto muda (apenas se a validação passar).
 * @param isReadOnly Se o campo deve ser apenas de leitura.
 * @param isRequired Se o campo é obrigatório (afeta o rótulo visual).
 * @param isError Se o campo está em estado de erro (afeta a cor da borda e exibe [errorMessage]).
 * @param errorMessage Mensagem de erro a ser exibida se [isError] for true.
 * @param keyboardOptions Opções de teclado, especialmente [KeyboardType.Password] e [KeyboardType.Number].
 * @param minLenght Comprimento/Valor mínimo para validação.
 * @param maxLenght Comprimento/Valor máximo para validação.
 */
@Composable
fun TextFieldOutline(
    label: String,
    value: String?,
    isSingleLine: Boolean = true,
    onValueChange: (String) -> Unit = {},
    isReadOnly: Boolean = false,
    isRequired: Boolean = false,
    isError: Boolean = false,
    errorMessage: String? = stringResource(id = R.string.mandatory_field),
    keyboardOptions: KeyboardOptions = KeyboardOptions.Default,
    minLenght: Int = 0,
    maxLenght: Int = Int.MAX_VALUE,
    textFieldModifier: Modifier = Modifier,
    modifier: Modifier = Modifier,
) {

    val labelText = formatRequiredLabel(label = label, isRequired = isRequired)
    val isPassword = keyboardOptions.keyboardType == KeyboardType.Password
    var passwordVisible by rememberSaveable { mutableStateOf(false) }

    val visualTransformation = if (isPassword) {
        if (passwordVisible) {
            VisualTransformation.None
        } else {
            PasswordVisualTransformation()
        }
    } else {
        VisualTransformation.None
    }

    Column(modifier = modifier) {
        OutlinedTextField(
            value = value ?: "",
            onValueChange = { newValue ->
                val isValid = isValueValid(
                    newValue = newValue,
                    keyboardOptions = keyboardOptions,
                    minLenght = minLenght,
                    maxLenght = maxLenght
                )

                if (isValid) {
                    onValueChange(newValue)
                }
            },
            singleLine = isSingleLine,
            readOnly = isReadOnly,
            isError = isError,
            label = { Text(text = labelText) },
            placeholder = { Text(text = labelText) },
            keyboardOptions = keyboardOptions,
            supportingText = {
                if (isError && errorMessage != null) {
                    Text(
                        text = errorMessage,
                        color = MaterialTheme.colorScheme.error,
                        style = MaterialTheme.typography.bodySmall
                    )
                }
            },
            visualTransformation = visualTransformation,
            trailingIcon = {
                if (isPassword) {
                    val image = if (passwordVisible) {
                        Icons.Filled.VisibilityOff
                    } else {
                        Icons.Filled.Visibility
                    }
                    val description =
                        if (passwordVisible) "Esconder password" else "Mostrar password"

                    IconButton(onClick = { passwordVisible = !passwordVisible }) {
                        Icon(imageVector = image, contentDescription = description)
                    }
                }
            },
            modifier = Modifier
                .fillMaxWidth()
                .then(textFieldModifier)
        )
    }
}

/**
 * Função privada de utilidade que verifica se o [newValue] cumpre as regras de validação.
 *
 * Inclui:
 * - Verificação de comprimento máximo para qualquer tipo de teclado.
 * - Validação específica para [KeyboardType.Number] (checa se o valor é um dígito e se
 * o valor numérico está dentro dos limites [minLenght] e [maxLenght]).
 * - Outros tipos de teclado (Email, Password, Phone) ainda têm validação pendente (TODO).
 *
 * @param newValue A nova string de texto inserida pelo usuário.
 * @param keyboardOptions O tipo de teclado usado (determina as regras de validação).
 * @param minLenght Comprimento/Valor mínimo aceitável.
 * @param maxLenght Comprimento/Valor máximo aceitável.
 * @return Retorna true se a string for válida para ser aceita, false caso contrário.
 */
private fun isValueValid(
    newValue: String,
    keyboardOptions: KeyboardOptions,
    minLenght: Int,
    maxLenght: Int
): Boolean {
    if (newValue.isEmpty()) {
        return true
    }

    val newValueLength = newValue.length

    if (newValue.length > maxLenght) {
        return false
    }

    when (keyboardOptions.keyboardType) {
        KeyboardType.Number -> {
            if (newValue.all { it.isDigit() } == false) {
                return false
            }

            val number = newValue.toInt()
            if (newValueLength >= minLenght.toString().length && number < minLenght) {
                return false
            }

            if (number > maxLenght) {
                return false
            }

            return true
        }

        KeyboardType.Email -> {
            if (newValue.any { it.isWhitespace() }) {
                return false
            }

            return true
        }

        KeyboardType.Phone -> {
            if (!newValue.all { it.isDigit() }) {
                return false
            }

            if (newValue.length > 9) {
                return false
            }

            return true
        }

        KeyboardType.Password -> {
            return true
        }

        else -> {
            return true
        }
    }
}
