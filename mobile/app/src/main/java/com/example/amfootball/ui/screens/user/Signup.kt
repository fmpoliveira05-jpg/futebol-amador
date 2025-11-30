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
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.rememberCoroutineScope
import androidx.compose.runtime.saveable.rememberSaveable
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
import com.example.amfootball.data.dtos.player.CreateProfileDto
import com.example.amfootball.data.enums.Position
import com.example.amfootball.ui.viewModel.auth.AuthViewModel
import com.example.amfootball.data.validators.validateSignUpForm
import com.example.amfootball.navigation.objects.Routes
import com.example.amfootball.ui.components.Loading
import com.example.amfootball.ui.components.inputFields.DatePickerDockedPastLimitedDate
import com.example.amfootball.ui.components.inputFields.EmailTextField
import com.example.amfootball.ui.components.inputFields.LabelSelectBox
import com.example.amfootball.ui.components.inputFields.PasswordTextField
import com.example.amfootball.ui.components.inputFields.PhoneInputWithDynamicCountries
import com.example.amfootball.ui.components.inputFields.TextFieldOutline
import com.example.amfootball.ui.theme.AMFootballTheme
import com.example.amfootball.utils.GeneralConst
import com.example.amfootball.utils.PlayerConst
import com.example.amfootball.utils.UserConst
import java.text.SimpleDateFormat
import java.util.Date
import java.util.Locale
import java.util.TimeZone

/**
 * Ecrã de Registo de Novo Utilizador.
 *
 * Este Composable é o ponto de entrada (Stateful) que conecta o [AuthViewModel] à UI.
 * Gere a navegação e delega a lógica de registo para o ViewModel.
 *
 * @param navHostController Controlador de navegação para transição entre ecrãs.
 * @param authViewModel ViewModel injetado pelo Hilt para gerir a autenticação.
 */
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
        onRegister = authViewModel::registerUser
    )
}

@Composable
private fun ContentSignUp(
    navController: NavHostController,
    onRegister: (CreateProfileDto, String, () -> Unit, (String) -> Unit) -> Unit,
    modifier: Modifier = Modifier
) {
    Column(
        modifier = modifier,
        verticalArrangement = Arrangement.Center,
        horizontalAlignment = Alignment.CenterHorizontally
    ) {
        FieldsSignUp(
            navHostController = navController,
            onRegister = onRegister
        )
    }
}

/**
 * Formulário de Registo contendo todos os campos de entrada e validações.
 *
 * @param navHostController Usado para navegação após sucesso.
 * @param onRegister Callback para executar a lógica de registo (recebe DTO, Password, Sucesso, Erro).
 */
@Composable
private fun FieldsSignUp(
    navHostController: NavHostController,
    onRegister: (CreateProfileDto, String, () -> Unit, (String) -> Unit) -> Unit
) {
    // --- Estados para os campos ---
    var email by rememberSaveable { mutableStateOf("") }
    var password by rememberSaveable { mutableStateOf("") }
    var passwordVerification by rememberSaveable { mutableStateOf("") }
    var name by rememberSaveable { mutableStateOf("") }
    var position by rememberSaveable { mutableStateOf<Int?>(null) }
    var dateOfBirth by rememberSaveable { mutableStateOf<Long?>(null) }
    var phone by rememberSaveable { mutableStateOf("") }
    var countryCode by rememberSaveable { mutableStateOf("+351") }
    var height by rememberSaveable { mutableStateOf("") }
    var address by rememberSaveable { mutableStateOf("") }

    // --- Estados da UI ---
    var showDatePicker by remember { mutableStateOf(false) }
    var isLoading by remember { mutableStateOf(false) }
    var errorMessage by remember { mutableStateOf<String?>(null) }

    // --- ViewModels e Scopes ---
    val scope = rememberCoroutineScope()
    val displayDateFormatter = remember {
        SimpleDateFormat("dd/MM/yyyy", Locale.getDefault())
    }

    val apiDateFormatter = remember {
        SimpleDateFormat("yyyy-MM-dd", Locale.getDefault()).apply {
            timeZone = TimeZone.getTimeZone("UTC")
        }
    }

    Spacer(Modifier.height(20.dp))

    TextFieldOutline(
        label = stringResource(id = R.string.player_name),
        value = name,
        onValueChange = { name = it },
        maxLenght = UserConst.MAX_NAME_LENGTH,
        isRequired = true
    )

    EmailTextField(
        value = email,
        onValueChange = { email = it },
        isError = false,
        errorMessage = ""
    )

    PhoneInputWithDynamicCountries(
        phoneNumber = phone,
        onPhoneNumberChange = { novoNumero ->
            if (novoNumero.length <= UserConst.SIZE_PHONE_NUMBER) {
                phone = novoNumero
            }
        },
        initialCountryCode = countryCode,
        onCountryCodeChange = { novoPais ->
            countryCode = novoPais.dialCode
        },
        isRequired = true,
        modifier = Modifier.fillMaxWidth()
    )

    TextFieldOutline(
        label = stringResource(id = R.string.player_size),
        value = height,
        onValueChange = { height = it },
        isRequired = true,
        maxLenght = PlayerConst.MAX_HEIGHT,
        keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number)
    )

    TextFieldOutline(
        label = stringResource(id = R.string.address),
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
                stringResource(R.string.player_position_picker)
            } else {
                stringResource(id = pos.stringId)
            }
        },
        modifier = Modifier.fillMaxWidth()
    )

    Spacer(Modifier.height(8.dp))

    Text(text = "${stringResource(R.string.date_of_birthday)}:")
    DatePickerDockedPastLimitedDate(
        value = if (dateOfBirth == null) "" else displayDateFormatter.format(Date(dateOfBirth!!)),
        onDateSelected = { selectedMillis ->
            dateOfBirth = selectedMillis
        },
        label = stringResource(R.string.date_of_birthday),
        contentDescription = stringResource(R.string.date_of_birthday_description),
        isSingleLine = true,
    )

    Spacer(Modifier.height(8.dp))

    PasswordTextField(
        label = stringResource(id = R.string.password_label),
        value = password,
        onValueChange = { password = it },
        isError = false,
        errorMessage = ""
    )

    PasswordTextField(
        label = stringResource(id = R.string.password_confirm),
        value = passwordVerification,
        onValueChange = { passwordVerification = it },
        isError = false,
        errorMessage = ""
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
                isLoading = true
                errorMessage = null
                val fullPhoneNumber = "$countryCode$phone"

                val userProfile = CreateProfileDto(
                    userName = name,
                    phone = fullPhoneNumber,
                    height = height.toInt(),
                    dateOfBirth = apiDateFormatter.format(Date(dateOfBirth!!)),
                    position = position!!,
                    address = address,
                    password = password,
                    email = email
                )

                onRegister(
                    userProfile,
                    password,
                    {
                        isLoading = false
                        navHostController.navigate(Routes.GeralRoutes.HOMEPAGE.route) {
                            popUpTo(navHostController.graph.startDestinationId) {
                                inclusive = true
                            }
                            launchSingleTop = true
                        }
                    },
                    { msgErro ->
                        isLoading = false
                        errorMessage = msgErro
                    }
                )
            }
        },
        modifier = Modifier.fillMaxWidth(),
        enabled = !isLoading
    ) {
        if (isLoading) {
            Loading()
        } else {
            Text(stringResource(id = R.string.signup))
        }
    }


}

@Preview(
    name = "SignUp Screen - EN",
    locale = "en",
    showBackground = true
)
@Preview(
    name = "SignUp Screen - PT",
    locale = "pt-rPT",
    showBackground = true
)
@Composable
fun SignUpScreenContentPreview() {
    AMFootballTheme {
        ContentSignUp(
            navController = rememberNavController(),
            onRegister = { _, _, onSuccess, _ ->
                onSuccess()
            }
        )
    }
}