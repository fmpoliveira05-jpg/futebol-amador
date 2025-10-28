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
import com.google.firebase.auth.auth
import com.google.firebase.Firebase
import android.util.Log
import com.example.amfootball.data.ApiClient.chamarApiDotNet

private lateinit var auth: FirebaseAuth

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun LoginScreen(navController: NavHostController){
    var email by remember { mutableStateOf("") }
    var password by remember { mutableStateOf("") }
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

            LabeledInputField(
                name = "Password",
                value = password,
                onValueChange = { newValue ->
                    password = newValue
                },
                label = "Digite a password",
                isPassword = true
            )

            //Depois meter logica para validar dados
            Button(onClick = {
                loginParaTestarAPI(email, password)
            }) {
                Text("Entrar")
            }
        }
    }
}

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun LabeledInputField(name: String,
                              value: String,
                              onValueChange: (String) -> Unit,
                              modifier: Modifier = Modifier,
                              label: String = name,
                              textFieldModifier: Modifier = Modifier.fillMaxWidth(),
                              isPassword: Boolean = false
) {
    Column(modifier = modifier) { // Agrupa o Text e o TextField
        Text(text = "$name:") // Mostra o nome acima
        Spacer(Modifier.height(4.dp)) // Pequeno espaço
        TextField( // Ou OutlinedTextField
            value = value,
            onValueChange = onValueChange,
            label = { Text(label) },
            modifier = textFieldModifier,
            singleLine = true,
            // Chama as funções auxiliares corrigidas
            visualTransformation = determineVisualTransformation(isPassword),
            keyboardOptions = determineKeyboardOptions(isPassword) // Nome mudado
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

private fun determineKeyboardOptions(isPassword: Boolean = false): KeyboardOptions{
    return KeyboardOptions(
        keyboardType = if (isPassword) {
            KeyboardType.Password
        } else {
            KeyboardType.Text
        }
    )
}

private fun loginParaTestarAPI(email: String, password: String) {

    auth.signInWithEmailAndPassword(email, password)
        .addOnCompleteListener { task ->
            if (task.isSuccessful) {
                // 1. LOGIN COM SUCESSO
                Log.d("LoginTeste", "Login bem-sucedido!")
                val user = auth.currentUser

                // 2. OBTER O TOKEN DE ID (JWT)
                user?.getIdToken(true) // O 'true' força a atualização do token
                    ?.addOnCompleteListener { tokenTask ->
                        if (tokenTask.isSuccessful) {
                            val token = tokenTask.result?.token

                            if (token != null) {
                                Log.d("LoginTeste", "--- TOKEN DO FIREBASE ---")
                                Log.d("LoginTeste", token)
                                Log.d("LoginTeste", "-------------------------")

                                // 3. ENVIAR O TOKEN PARA A SUA API .NET
                                chamarApiDotNet(token)
                            }
                        } else {
                            Log.w("LoginTeste", "Falha ao obter token:", tokenTask.exception)
                        }
                    }
            } else {
                // Falha no login
                Log.w("LoginTeste", "Falha no login:") // apos a mensagem (segundo campo) adicionar uma , e por uma exceptiomn
            }
        }
}