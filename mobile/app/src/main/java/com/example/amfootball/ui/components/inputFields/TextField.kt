package com.example.amfootball.ui.components.inputFields

import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Visibility
import androidx.compose.material.icons.filled.VisibilityOff
import androidx.compose.material3.ExperimentalMaterial3Api
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

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun LabelTextField(
    label: String,
    value: String?,
    modifier: Modifier = Modifier,
    isSingleLine: Boolean = true,
    onValueChange: (String) -> Unit = {},
    isReadOnly: Boolean = false,
    isRequired: Boolean = false,
    isError: Boolean = false,
    errorMessage: String = stringResource(id = R.string.mandatory_field),
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
            maxLenght = maxLenght
        )
    }
}

@Composable
fun TextFieldOutline(
    label: String,
    value: String?,
    modifier: Modifier = Modifier,
    isSingleLine: Boolean = true,
    onValueChange: (String) -> Unit = {},
    isReadOnly: Boolean = false,
    isRequired: Boolean = false,
    isError: Boolean = false,
    errorMessage: String? = stringResource(id = R.string.mandatory_field),
    keyboardOptions: KeyboardOptions = KeyboardOptions.Default,
    minLenght: Int = 0,
    maxLenght: Int = Int.MAX_VALUE,
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
            modifier = Modifier.fillMaxWidth(),
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
                    val description = if (passwordVisible) "Esconder password" else "Mostrar password"

                    IconButton(onClick = { passwordVisible = !passwordVisible }) {
                        Icon(imageVector = image, contentDescription = description)
                    }
                }
            }
        )
    }
}

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
            if(newValueLength >= minLenght.toString().length && number < minLenght){
                return false
            }

            if(number > maxLenght){
                return false
            }

            return true
        }

        KeyboardType.Email -> {
            //TODO: Validações de email
            return true
        }

        KeyboardType.Password -> {
            //TODO: Meter validações de password
            return true
        }

        KeyboardType.Phone -> {
            //TODO: Meter validações de telefone
            return true
        }
        else -> {
            return true
        }
    }
}
