package com.example.amfootball.ui.screens.user

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.Button
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.rememberCoroutineScope
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.hilt.navigation.compose.hiltViewModel
import androidx.navigation.NavHostController
import androidx.navigation.compose.rememberNavController
import com.example.amfootball.R
import com.example.amfootball.data.dtos.CreateProfileDto
import com.example.amfootball.data.enums.Position
import com.example.amfootball.ui.components.inputFields.DatePickerModalInput
import com.example.amfootball.ui.viewModel.AuthViewModel
import com.example.amfootball.data.validators.validateSignUpForm
import com.example.amfootball.navigation.objects.Routes
import com.example.amfootball.ui.components.inputFields.EmailTextField
import com.example.amfootball.ui.components.inputFields.LabelSelectBox
import com.example.amfootball.ui.components.inputFields.PasswordTextField
import com.example.amfootball.ui.components.inputFields.TextFieldOutline
import com.example.amfootball.ui.theme.AMFootballTheme
import com.example.amfootball.utils.GeneralConst
import com.example.amfootball.utils.PlayerConst
import com.example.amfootball.utils.UserConst
import kotlinx.coroutines.launch
import java.text.SimpleDateFormat
import java.util.Date
import java.util.Locale
import java.util.TimeZone

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun SignUpScreen(
    navHostController: NavHostController,
    authViewModel: AuthViewModel = hiltViewModel()
) {
    ContentSignUp(
        modifier = Modifier
        .fillMaxSize()
        .padding(16.dp)
        .verticalScroll(rememberScrollState()),
        navController = navHostController,
        authViewModel = authViewModel
    )
}

@Composable
private fun ContentSignUp(
    navController: NavHostController,
    authViewModel: AuthViewModel = hiltViewModel(),
    modifier: Modifier = Modifier
) {
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
private fun FieldsSignUp(
    navHostController: NavHostController,
    authViewModel: AuthViewModel = hiltViewModel()
) {
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

    // --- ViewModels e Scopes ---
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

    TextFieldOutline(
        label = "Name",
        value = name,
        onValueChange = { name = it },
        maxLenght = UserConst.MAX_NAME_LENGTH,
        isRequired = true
    )

    EmailTextField(
        value = email,
        onValueChange = { email = it },
    )

    TextFieldOutline(
        label = "Phone",
        value = phone,
        onValueChange = { phone = it },
        isRequired = true,
        maxLenght = UserConst.SIZE_PHONE_NUMBER,
        keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Phone)
    )

    TextFieldOutline(
        label = "Height",
        value = height,
        onValueChange = { height = it },
        isRequired = true,
        maxLenght = PlayerConst.MAX_HEIGHT,
        keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number)
    )

    TextFieldOutline(
        label = "Address",
        value = address,
        maxLenght = GeneralConst.MAX_ADDRESS_LENGTH,
        onValueChange = { address = it },
        isRequired = true,
    )

    Spacer(Modifier.height(8.dp))
    Spacer(Modifier.height(4.dp))

    val listPosition: List<Position> = Position.values().toList()
    val selectedPositionObject: Position? = listPosition.find { it.ordinal == position }

    LabelSelectBox(
        label = stringResource(id = R.string.filter_position),
        list = listPosition,
        selectedValue = selectedPositionObject,
        onSelectItem = { selectedPosition ->
            position = selectedPosition?.ordinal
        },
        itemToString = { pos ->
            if (pos == null) {
                "Escolha uma Posição"
            } else {
                stringResource(id = pos.stringId)
            }
        },
        modifier = Modifier.fillMaxWidth()
    )

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

    PasswordTextField(
        value = password,
        onValueChange = { password = it }
    )

    PasswordTextField(
        label = "Password Verification",
        value = passwordVerification,
        onValueChange = { passwordVerification = it },
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
                        authViewModel.registerUser(userProfile, password)

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

@Preview(
    name = "SignUp Screen - EN",
    locale = "en",
    showBackground = true)
@Preview(
    name = "SignUp Screen - PT",
    locale = "pt",
    showBackground = true)
@Composable
fun SignUpScreenContentPreview() {
    AMFootballTheme {
        SignUpScreen(rememberNavController())
    }
}
