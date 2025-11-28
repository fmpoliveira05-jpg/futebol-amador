package com.example.amfootball.ui.screens.user

import android.widget.Toast
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.Button
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import androidx.compose.runtime.rememberCoroutineScope
import androidx.compose.ui.Alignment
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.tooling.preview.Preview
import androidx.hilt.navigation.compose.hiltViewModel
import androidx.navigation.NavHostController
import androidx.navigation.compose.rememberNavController
import com.example.amfootball.R
import com.example.amfootball.navigation.objects.Routes
import com.example.amfootball.ui.components.inputFields.EmailTextField
import com.example.amfootball.ui.components.inputFields.PasswordTextField
import com.example.amfootball.ui.viewModel.AuthViewModel

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun LoginScreen(
    navHostController: NavHostController,
    authViewModel: AuthViewModel = hiltViewModel()
){
    ContentLogin(
        modifier = Modifier
            .padding(16.dp),
        navHostController = navHostController,
        authViewModel = authViewModel
    )
}

@Composable
private fun ContentLogin(navHostController: NavHostController,
                         authViewModel: AuthViewModel,
                         modifier: Modifier = Modifier
) {
    Column(
        modifier = modifier,
        verticalArrangement = Arrangement.Center,
        horizontalAlignment = Alignment.CenterHorizontally
    ) {
        FieldsLogin(
            navHostController = navHostController,
            authViewModel = authViewModel
        )
    }
}

@Composable
private fun FieldsLogin(
    navHostController: NavHostController,
    authViewModel: AuthViewModel
) {
    var email by remember { mutableStateOf("") }
    var password by remember { mutableStateOf("") }

    // --- 2. Obter um CoroutineScope para chamar funções 'suspend' ---
    val scope = rememberCoroutineScope()

    // --- 3. Estados para feedback ao utilizador ---
    var isLoading by remember { mutableStateOf(false) }
    var errorMessage by remember { mutableStateOf<String?>(null) }

    Text(text = "Conteúdo do Ecrã de Login")
    Spacer(Modifier.height(20.dp))

    EmailTextField(
        value = email,
        onValueChange = { newValue ->
            email = newValue
        }
    )

    Spacer(Modifier.height(8.dp))

    PasswordTextField(
        value = password,
        onValueChange = { newValue ->
            password = newValue
        }
    )

    Spacer(Modifier.height(16.dp))

    // --- 4. Mostrar a mensagem de erro, se existir ---
    if (errorMessage != null) {
        Text(
            text = errorMessage!!,
            color = Color.Red,
            modifier = Modifier.padding(bottom = 8.dp)
        )
    }

    val context = LocalContext.current
    val errText = stringResource(R.string.error_login)
    Button(
        onClick = {
            authViewModel.loginUser(email, password) { loginComSucesso ->
                if (loginComSucesso) {
                    navHostController.navigate(Routes.GeralRoutes.HOMEPAGE.route) {
                        popUpTo(navHostController.graph.startDestinationId) { inclusive = true }
                    }
                } else {
                    Toast.makeText(
                        context,
                        errText,
                        Toast.LENGTH_LONG
                    ).show()
                }
            }
        },
        modifier = Modifier.fillMaxWidth()
    ) {
        Text("Entrar")
    }
}

@Preview(name = "Login Screen - EN", locale = "en", showBackground = true)
@Preview(name = "Login Screen - PT", locale = "pt", showBackground = true)
@Composable
fun LoginScreenPreview() {
    LoginScreen(navHostController = rememberNavController())
}