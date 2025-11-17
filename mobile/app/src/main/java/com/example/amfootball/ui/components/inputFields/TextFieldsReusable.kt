package com.example.amfootball.ui.components.inputFields

import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.runtime.Composable
import androidx.compose.ui.text.input.KeyboardType
import com.example.amfootball.utils.UserConst

@Composable
fun EmailTextField(
    value: String,
    onValueChange: (String) -> Unit,
    isRequired: Boolean = true
) {
    TextFieldOutline(
        label = "Email",
        value = value,
        onValueChange = onValueChange,
        isRequired = isRequired,
        maxLenght = UserConst.MAX_EMAIL_LENGTH,
        keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Email)
    )
}

@Composable
fun PasswordTextField(
    label: String = "Password",
    value: String,
    onValueChange: (String) -> Unit,
    isRequired: Boolean = true
) {
    TextFieldOutline(
        label = label,
        value = value,
        onValueChange = onValueChange,
        isRequired = isRequired,
        keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Password)
    )
}