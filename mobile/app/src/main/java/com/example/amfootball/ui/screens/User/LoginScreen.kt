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
import androidx.compose.material3.TopAppBar
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.text.input.PasswordVisualTransformation
import androidx.compose.ui.text.input.VisualTransformation
import androidx.compose.ui.unit.dp
import androidx.navigation.NavHostController
import com.example.amfootball.ui.components.Buttons.BackButton
import com.google.firebase.auth.FirebaseAuth
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.runtime.rememberCoroutineScope
import androidx.compose.ui.graphics.Color
import androidx.lifecycle.viewmodel.compose.viewModel
import com.example.amfootball.navigation.Objects.NavBar.RouteNavBarHomePage
import com.example.amfootball.ui.components.InputFields.LabeledInputField
import kotlinx.coroutines.launch
import com.example.amfootball.ui.viewModel.AuthViewModel

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun LoginScreen(navController: NavHostController){
    var email by remember { mutableStateOf("") }
    var password by remember { mutableStateOf("") }
    val authViewModel: AuthViewModel = viewModel()
    // --- 2. Obter um CoroutineScope para chamar funções 'suspend' ---
    // (O onClick de um botão não é um scope por defeito)
    val scope = rememberCoroutineScope()
    // --- 3. Estados para feedback ao utilizador ---
    var isLoading by remember { mutableStateOf(false) }
    var errorMessage by remember { mutableStateOf<String?>(null) }

    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text("Login") },
                navigationIcon = {
                    BackButton(navController = navController)
                }
                //Poderei meter actions
            )
        }
    ) { paddingValues -> // O conteúdo do ecrã vai aqui dentro
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(paddingValues)
                .padding(16.dp),
            verticalArrangement = Arrangement.Center,
            horizontalAlignment = androidx.compose.ui.Alignment.CenterHorizontally
        ) {

            Text(text = "Conteúdo do Ecrã de Login")
            Spacer(Modifier.height(20.dp))

            LabeledInputField(
                name = "Email",
                value = email,
                onValueChange = { newValue ->
                    email = newValue
                },
                label = "Digite o seu email",
                isPassword = false
            )

            Spacer(Modifier.height(8.dp))

            LabeledInputField(
                name = "Password",
                value = password,
                onValueChange = { newValue ->
                    password = newValue
                },
                label = "Digite a password",
                isPassword = true
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

            //Depois meter logica para validar dados
            Button(onClick = {
                if (email.isNotBlank() && password.isNotBlank()){
                    isLoading = true
                    errorMessage = null

                    scope.launch {
                        val loginSucesso = authViewModel.loginUser(email, password)
                        isLoading = false

                        if (loginSucesso){
                            navController.navigate(RouteNavBarHomePage.HOME_PAGE) {
                                popUpTo(navController.graph.startDestinationId){
                                    inclusive = true
                                }
                                launchSingleTop = true
                            }
                        } else {
                            //Lemmbrar de fazer as cenas da lingua como o artur disse
                            errorMessage = "Email ou password inválidos"
                        }
                    }
                } else {
                    errorMessage = "Por favor, preencha todos os campos."
                }

            }) {
                if (isLoading){
                    CircularProgressIndicator(modifier = Modifier.height(24.dp))
                } else {
                    Text("Entrar")
                }
            }
        }
    }
}