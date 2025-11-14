package com.example.amfootball.ui.components.inputFields

import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Visibility
import androidx.compose.material.icons.filled.VisibilityOff
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.Text
import androidx.compose.material3.TextField
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.saveable.rememberSaveable
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.text.input.PasswordVisualTransformation
import androidx.compose.ui.text.input.VisualTransformation
import androidx.compose.ui.unit.dp

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun LabeledInputField(
    name: String,
    value: String,
    onValueChange: (String) -> Unit,
    modifier: Modifier = Modifier,
    label: String = name,
    isPassword: Boolean = false,
    isEmailAddress: Boolean = false,
    isPhoneNumber: Boolean = false,
    keyboardType: KeyboardType = KeyboardType.Text
) {

    var passwordVisible by rememberSaveable { mutableStateOf(false) }

    val visualTransformation = if (isPassword) {
        if (passwordVisible) VisualTransformation.None else PasswordVisualTransformation()
    } else {
        VisualTransformation.None
    }

    //adicionar o alerta para quando o email ou a password forem invalidos
    var finalKeyboardType = keyboardType
    if (isPassword) {
        finalKeyboardType = KeyboardType.Password
    } else if (isPhoneNumber){
        finalKeyboardType = KeyboardType.Phone
    } else if (isEmailAddress) {
        finalKeyboardType = KeyboardType.Email
    }

    Column(modifier = modifier) {
        Text(text = "$name:") // A sua etiqueta (label) acima
        Spacer(Modifier.height(4.dp))
        TextField(
            value = value,
            onValueChange = onValueChange,
            label = { Text(label) }, // O label flutuante
            modifier = Modifier.fillMaxWidth(),
            singleLine = true,
            visualTransformation = visualTransformation,
            keyboardOptions = KeyboardOptions(keyboardType = finalKeyboardType),

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

