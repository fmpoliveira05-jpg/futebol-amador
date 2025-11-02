package com.example.amfootball.ui.screens.User

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.material3.Button
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.material3.TextField
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.text.input.PasswordVisualTransformation
import androidx.compose.ui.text.input.VisualTransformation
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.navigation.NavHostController
import androidx.navigation.compose.rememberNavController
import com.example.amfootball.R
import com.example.amfootball.ui.components.BackTopBar

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun LoginScreen(navController: NavHostController){

    Scaffold(
        topBar = { BackTopBar(navController = navController, title = stringResource(id = R.string.back_button_title)) }
    ) { paddingValues ->
        LoginContent(modifier = Modifier
            .padding(paddingValues)
            .padding(16.dp))
    }
}

@OptIn(ExperimentalMaterial3Api::class)
@Composable
private fun LoginContent(modifier: Modifier = Modifier) {
    var email by remember { mutableStateOf("") }
    var password by remember { mutableStateOf("") }

    Column(
        modifier = modifier.fillMaxSize(),
        verticalArrangement = Arrangement.Center,
        horizontalAlignment = androidx.compose.ui.Alignment.CenterHorizontally
    ) {
        Text(text = stringResource(id = R.string.login_Title))
        Spacer(Modifier.height(20.dp))

        LoginInputFields(
            email = email,
            password = password,
            onEmailChange = { newValue ->
                email = newValue
            },
            onPasswordChange = { newValue ->
                password = newValue
            }
        )

        Button(onClick = {  }) {
            Text(text = stringResource(id = R.string.login_button))
        }
    }
}

@Composable
private fun LoginInputFields(
    email: String,
    password: String,
    onEmailChange: (String) -> Unit,
    onPasswordChange: (String) -> Unit
) {
    LabeledInputField(
        name = stringResource(id = R.string.email_field),
        value = email,
        onValueChange = onEmailChange,
        label = stringResource(id = R.string.email_label),
        isPassword = false
    )

    LabeledInputField(
        name = stringResource(id = R.string.password_field),
        value = password,
        onValueChange = onPasswordChange,
        label = stringResource(id = R.string.password_label),
        isPassword = true
    )
}
@OptIn(ExperimentalMaterial3Api::class)
@Composable
private fun LabeledInputField(name: String,
                              value: String,
                              onValueChange: (String) -> Unit,
                              modifier: Modifier = Modifier,
                              label: String = name,
                              textFieldModifier: Modifier = Modifier.fillMaxWidth(),
                              isPassword: Boolean = false
) {
    Column(modifier = modifier) {
        Text(text = "$name:")
        Spacer(Modifier.height(4.dp))
        TextField(
            value = value,
            onValueChange = onValueChange,
            label = { Text(label) },
            modifier = textFieldModifier,
            singleLine = true,
            visualTransformation = determineVisualTransformation(isPassword),
            keyboardOptions = determineKeyboardOptions(isPassword)
        )
    }
}

private fun determineVisualTransformation(isPassword: Boolean = false): VisualTransformation {
    return if (isPassword) {
        PasswordVisualTransformation()
    } else {
        VisualTransformation.None
    }
}

private fun determineKeyboardOptions(isPassword: Boolean = false): KeyboardOptions {
    return KeyboardOptions(
        keyboardType = if (isPassword) {
            KeyboardType.Password
        } else {
            KeyboardType.Text
        }
    )
}

@Preview(
    name = "Default (EN)",
    locale = "en",
    showBackground = true,
)
@Preview(
    name = "Português (PT)",
    locale = "pt",
    showBackground = true,
)
@Composable
fun PreviewLoginScreen() {
    val fakeNavController = rememberNavController()
    LoginScreen(navController = fakeNavController)
}