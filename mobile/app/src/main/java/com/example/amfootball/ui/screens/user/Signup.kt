package com.example.amfootball.ui.screens.user

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.Button
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.DropdownMenuItem
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.ExposedDropdownMenuBox
import androidx.compose.material3.ExposedDropdownMenuDefaults
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.material3.TextField
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.rememberCoroutineScope
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.lifecycle.viewmodel.compose.viewModel
import androidx.navigation.NavHostController
import androidx.navigation.compose.rememberNavController
import com.example.amfootball.data.dtos.CreateProfileDto
import com.example.amfootball.data.enums.Position
import com.example.amfootball.ui.components.inputFields.DatePickerModalInput
import com.example.amfootball.ui.components.inputFields.LabeledInputField
import com.example.amfootball.ui.viewModel.AuthViewModel
import com.example.amfootball.data.validators.validateSignUpForm
import com.example.amfootball.navigation.Objects.Routes
import com.example.amfootball.ui.components.BackTopBar
import com.example.amfootball.ui.theme.AMFootballTheme
import kotlinx.coroutines.launch
import java.text.SimpleDateFormat
import java.util.Date
import java.util.Locale
import java.util.TimeZone

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun SignUpScreen(navHostController: NavHostController) {
    Scaffold(
        topBar = {
            BackTopBar(
                navHostController = navHostController,
                title = "Registo"
            )
        }
    ) { paddingValues ->
        ContentSignUp(
            modifier = Modifier
            .fillMaxSize()
            .padding(paddingValues)
            .padding(16.dp)
            .verticalScroll(rememberScrollState()),
            navController = navHostController
        )
    }
}

@Composable
private fun ContentSignUp(modifier: Modifier = Modifier,
                          navController: NavHostController) {
    Column(
        modifier = modifier,
        verticalArrangement = Arrangement.Center,
        horizontalAlignment = Alignment.CenterHorizontally
    ) {
        FieldsSignUp(navHostController = navController)
    }
}

@OptIn(ExperimentalMaterial3Api::class)
@Composable
private fun FieldsSignUp(navHostController: NavHostController) {
    // --- Estados para os campos ---
    var email by remember { mutableStateOf("") }
    var password by remember { mutableStateOf("") }
    var passwordVerification by remember { mutableStateOf("") }
    var name by remember { mutableStateOf("") }
    var position by remember { mutableStateOf<Int?>(null) }
    var dateOfBirth by remember { mutableStateOf<Long?>(null) }
    var phone by remember { mutableStateOf("") }
    var height by remember { mutableStateOf("") }
    var address by remember { mutableStateOf("") }

    // --- Estados da UI ---
    var showDatePicker by remember { mutableStateOf(false) }
    var isLoading by remember { mutableStateOf(false) }
    var errorMessage by remember { mutableStateOf<String?>(null) }
    var isPositionMenuExpanded by remember { mutableStateOf(false) }

    // --- ViewModels e Scopes ---
    val authViewModel: AuthViewModel = viewModel()
    val scope = rememberCoroutineScope()
    val displayDateFormatter = remember {
        SimpleDateFormat("dd/MM/yyyy", Locale.getDefault())
    }

    // Formato para enviar para a API (ex: 2025-10-30)
    val apiDateFormatter = remember {
        SimpleDateFormat("yyyy-MM-dd", Locale.getDefault()).apply {
            timeZone = TimeZone.getTimeZone("UTC")
        }
    }

    Text(text = "Criar nova conta")
    Spacer(Modifier.height(20.dp))

    LabeledInputField(
        name = "Name",
        value = name,
        onValueChange = { name = it },
        label = "Insira o seu nome"
    )

    LabeledInputField(
        name = "Phone",
        value = phone,
        onValueChange = { phone = it },
        label = "Telemóvel (9 dígitos)",
        keyboardType = KeyboardType.Phone
    )

    LabeledInputField(
        name = "Height",
        value = height,
        onValueChange = { height = it },
        label = "Altura (em cm)",
        keyboardType = KeyboardType.Number
    )

    LabeledInputField(
        name = "Morada",
        value = address,
        onValueChange = { address = it },
        label = "Insira a sua morada",
    )

    LabeledInputField(
        name = "Email",
        value = email,
        onValueChange = { email = it },
        label = "exemple@example.com",
        keyboardType = KeyboardType.Email
    )

    Spacer(Modifier.height(8.dp))
    Text(text = "Position:")
    Spacer(Modifier.height(4.dp))

    ExposedDropdownMenuBox(
        expanded = isPositionMenuExpanded,
        onExpandedChange = { isPositionMenuExpanded = !isPositionMenuExpanded },
        modifier = Modifier.fillMaxWidth()
    ) {
        TextField(
            value = Position.values().find { it.ordinal == position }?.displayName ?: "",
            onValueChange = {},
            readOnly = true,
            label = { Text("Selecione a Posição") },
            trailingIcon = {
                ExposedDropdownMenuDefaults.TrailingIcon(expanded = isPositionMenuExpanded)
            },
            modifier = Modifier
                .menuAnchor() // Liga o TextField ao menu
                .fillMaxWidth()
        )

        // O menu que aparece
        ExposedDropdownMenu(
            expanded = isPositionMenuExpanded,
            onDismissRequest = { isPositionMenuExpanded = false }
        ) {
            // Itera sobre o seu enum
            Position.values().forEach { pos ->
                DropdownMenuItem(
                    text = { Text(pos.displayName) }, // Mostra "Avançado", "Médio", etc.
                    onClick = {
                        position = pos.ordinal // Guarda "FORWARD", "MIDFIELER", etc.
                        isPositionMenuExpanded = false // Fecha o menu
                    }
                )
            }
        }
    }

    Spacer(Modifier.height(8.dp))
    // Data de nascimento
    Text(text = "Data de Nascimento:")
    Button(onClick = { showDatePicker = true }, modifier = Modifier.fillMaxWidth()) {
        // Mostra a data formatada se não for nula
        val displayDate = dateOfBirth?.let {
            displayDateFormatter.format(Date(it))
        } ?: "Selecionar Data"
        Text(displayDate)
    }

    if (showDatePicker) {
        DatePickerModalInput( // Ou DatePickerModal
            onDateSelected = { selectedMillis ->
                // O 'selectedMillis' já está em UTC, guarde-o diretamente
                dateOfBirth = selectedMillis
            },
            onDismiss = { showDatePicker = false }
        )
    }
    // --- Fim do Campo de Data ---

    Spacer(Modifier.height(8.dp))

    LabeledInputField(
        name = "Password",
        value = password,
        onValueChange = { password = it },
        label = "Digite a password",
        isPassword = true
    )

    LabeledInputField(
        name = "Password Verification",
        value = passwordVerification,
        onValueChange = { passwordVerification = it },
        label = "Confirme a sua password",
        isPassword = true
    )

    Spacer(Modifier.height(16.dp))

    if (errorMessage != null) {
        Text(
            text = errorMessage!!,
            color = Color.Red,
            modifier = Modifier.padding(bottom = 8.dp)
        )
    }

    Button(
        onClick = {
            val validationResult = validateSignUpForm(
                name = name,
                phone = phone,
                height = height,
                email = email,
                password = password,
                passwordVerification = passwordVerification,
                dateOfBirth = dateOfBirth,
                position = position
            )

            if (!validationResult.isValid) {
                errorMessage = "${validationResult.fieldName}: ${validationResult.errorMessage}"
            } else {
                // Validação OK!
                isLoading = true
                errorMessage = null

                val userProfile = CreateProfileDto(
                    userName = name,
                    phone = phone,
                    height = height.toInt(),
                    dateOfBirth = apiDateFormatter.format(Date(dateOfBirth!!)),
                    position = position!!,
                    address = address,
                    email = email
                )

                scope.launch {
                    try {
                        // Chamar a função de registo, não a de login
                        authViewModel.registerUser(userProfile, password)

                        // Sucesso!
                        isLoading = false
                        navHostController.navigate(Routes.GeralRoutes.HOMEPAGE.route) {
                            popUpTo(navHostController.graph.startDestinationId) {
                                inclusive = true
                            }
                            launchSingleTop = true
                        }
                    } catch (e: Exception) {
                        // Erro (do ViewModel)
                        isLoading = false
                        errorMessage = e.message ?: "Ocorreu um erro no registo."
                    }
                }
            }
        },
        modifier = Modifier.fillMaxWidth(),
        enabled = !isLoading
    ) {
        if (isLoading) {
            CircularProgressIndicator(modifier = Modifier.height(24.dp))
        } else {
            Text("Registar")
        }
    }
}

@Preview(name = "SignUp Screen - EN", locale = "en", showBackground = true)
@Preview(name = "SignUp Screen - PT", locale = "pt", showBackground = true)
@Composable
fun SignUpScreenContentPreview() {
    AMFootballTheme {
        SignUpScreen(rememberNavController())
    }
}
