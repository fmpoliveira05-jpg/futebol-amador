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
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.saveable.rememberSaveable
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.platform.testTag
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.unit.dp
import androidx.hilt.navigation.compose.hiltViewModel
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import androidx.navigation.NavHostController
import com.example.amfootball.R
import com.example.amfootball.data.dtos.player.CreateProfileDto
import com.example.amfootball.data.enums.Position
import com.example.amfootball.data.errors.formErrors.SignUpFormErrors
import com.example.amfootball.data.validators.validateSignUpForm
import com.example.amfootball.navigation.objects.Routes
import com.example.amfootball.ui.components.Loading
import com.example.amfootball.ui.components.inputFields.DatePickerDockedPastLimitedDate
import com.example.amfootball.ui.components.inputFields.EmailTextField
import com.example.amfootball.ui.components.inputFields.LabelSelectBox
import com.example.amfootball.ui.components.inputFields.PasswordTextField
import com.example.amfootball.ui.components.inputFields.PhoneInputWithDynamicCountries
import com.example.amfootball.ui.components.inputFields.TextFieldOutline
import com.example.amfootball.ui.components.notification.showOfflineToast
import com.example.amfootball.ui.viewModel.auth.SignupViewmodel
import com.example.amfootball.utils.GeneralConst
import com.example.amfootball.utils.PlayerConst
import com.example.amfootball.utils.UserConst
import java.text.SimpleDateFormat
import java.util.Date
import java.util.Locale
import java.util.TimeZone

/**
 * Ecrã de Registo de Novo Utilizador (Sign Up).
 *
 * Este Composable é o ponto de entrada (Stateful) que liga o ViewModel à UI.
 * A principal responsabilidade é iniciar o formulário e delegar a renderização do conteúdo.
 *
 * @param navHostController Controlador de navegação para transição entre ecrãs (após sucesso).
 * @param authViewModel ViewModel injetado pelo Hilt para gerir a lógica de registo.
 */
@Composable
fun SignUpScreen(
    navHostController: NavHostController,
    viewModel: SignupViewmodel = hiltViewModel()
) {
    // --- Observar Estados do ViewModel ---
    val uiFormState by viewModel.uiFormState.collectAsStateWithLifecycle()
    val uiErrors by viewModel.uiFormErrors.collectAsStateWithLifecycle()
    val uiState by viewModel.uiState.collectAsStateWithLifecycle() // Loading e Erros Globais
    val isOnline by viewModel.isOnline.collectAsStateWithLifecycle()

    // Estados auxiliares do VM
    val countryCode by viewModel.countryCode.collectAsStateWithLifecycle()
    val passwordVerification by viewModel.passwordVerification.collectAsStateWithLifecycle()

    val context = LocalContext.current

    ContentSignUp(
        modifier = Modifier
            .fillMaxSize()
            .padding(16.dp)
            .verticalScroll(rememberScrollState()),
        formDto = uiFormState,
        formErrors = uiErrors,
        countryCode = countryCode,
        passwordVerification = passwordVerification,
        isLoading = uiState.isLoading,
        globalErrorMessage = uiState.errorMessage,
        viewModel = viewModel, // Passamos o VM para aceder aos setters
        onSubmit = {
            viewModel.onSubmit(navHostController)
        }
    )
}

@Composable
private fun ContentSignUp(
    formDto: CreateProfileDto,
    formErrors: SignUpFormErrors,
    countryCode: String,
    passwordVerification: String,
    isLoading: Boolean,
    globalErrorMessage: String?,
    viewModel: SignupViewmodel,
    onSubmit: () -> Unit,
    modifier: Modifier = Modifier
) {
    Column(
        modifier = modifier,
        verticalArrangement = Arrangement.Center,
        horizontalAlignment = Alignment.CenterHorizontally
    ) {

        Spacer(Modifier.height(20.dp))

        // NOME
        TextFieldOutline(
            label = stringResource(id = R.string.player_name),
            value = formDto.userName,
            onValueChange = viewModel::onNameChange,
            maxLenght = UserConst.MAX_NAME_LENGTH,
            isRequired = true,
            isError = formErrors.nameError != null,
            errorMessage = formErrors.nameError?.let { stringResource(id = it.messageId, *it.args.toTypedArray()) }
        )

        // EMAIL
        EmailTextField(
            value = formDto.email,
            onValueChange = viewModel::onEmailChange,
            isError = formErrors.emailError != null,
            errorMessage = formErrors.emailError?.let { stringResource(id = it.messageId, *it.args.toTypedArray()) } ?: "",
            textFieldModifier = Modifier.testTag(stringResource(id = R.string.tag_email_input))
        )

        // TELEMÓVEL
        PhoneInputWithDynamicCountries(
            phoneNumber = formDto.phone,
            onPhoneNumberChange = { novoNumero ->
                if (novoNumero.length <= UserConst.SIZE_PHONE_NUMBER) {
                    viewModel.onPhoneChange(novoNumero)
                }
            },
            initialCountryCode = countryCode,
            onCountryCodeChange = { novoPais ->
                viewModel.onCountryCodeChange(novoPais.dialCode)
            },
            isRequired = true,
            modifier = Modifier.fillMaxWidth(),
            isError = formErrors.phoneError != null,
            errorMessage = formErrors.phoneError?.let { stringResource(id = it.messageId, *it.args.toTypedArray()) }
        )

        // MORADA

        TextFieldOutline(
            label = stringResource(id = R.string.address),
            value = formDto.address,
            maxLenght = GeneralConst.MAX_ADDRESS_LENGTH,
            onValueChange = viewModel::onAddressChange,
            isRequired = true,
            isError = formErrors.addressError != null,
            errorMessage = formErrors.addressError?.let { stringResource(id = it.messageId, *it.args.toTypedArray()) }
        )

        // ALTURA
        TextFieldOutline(
            label = stringResource(id = R.string.player_size),
            value = if (formDto.height == 0) "" else formDto.height.toString(),
            onValueChange = viewModel::onHeightChange,
            isRequired = true,
            maxLenght = PlayerConst.MAX_HEIGHT,
            keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number),
            isError = formErrors.heightError != null,
            errorMessage = formErrors.heightError?.let { stringResource(id = it.messageId, *it.args.toTypedArray()) }
        )

        Spacer(Modifier.height(8.dp))

        // POSIÇÃO
        val listPosition: List<Position> = Position.values().toList()
        val selectedPositionObject: Position? = listPosition.find { it.ordinal == formDto.position }

        LabelSelectBox(
            label = stringResource(id = R.string.filter_position),
            list = listPosition,
            selectedValue = selectedPositionObject,
            onSelectItem = { selectedPosition ->
                viewModel.onPositionChange(selectedPosition?.ordinal)
            },
            itemToString = { pos ->
                if (pos == null) stringResource(R.string.player_position_picker)
                else stringResource(id = pos.stringId)
            },
            modifier = Modifier.fillMaxWidth()
        )
        if(formErrors.positionError != null) {
            Text(text = stringResource(id = formErrors.positionError.messageId), color = Color.Red)
        }

        Spacer(Modifier.height(8.dp))

        // DATA DE NASCIMENTO
        Text(text = "${stringResource(R.string.date_of_birthday)}:")
        // Aqui convertemos a string do DTO de volta para visualização se necessário,
        // ou usamos a lógica do displayDateFormatter do VM
        val displayDate = if (formDto.dateOfBirth.isBlank()) "" else {
            // Pequeno truque: se a string já estiver formatada em yyyy-MM-dd,
            // talvez precises de converter para dd/MM/yyyy para mostrar.
            // Como tens o DatePickerDockedPastLimitedDate a receber value, assume-se que é String.
            // O ideal seria usar o valor formatado para display.
            formDto.dateOfBirth // Ou viewModel.displayDateFormatter.format(...) se tiveres guardado o long
        }

        DatePickerDockedPastLimitedDate(
            value = displayDate,
            onDateSelected = viewModel::onDateChange, // Passa o Long para o VM
            label = stringResource(R.string.date_of_birthday),
            contentDescription = stringResource(R.string.date_of_birthday_description),
            isSingleLine = true,
        )
        if(formErrors.dateError != null) {
            Text(text = stringResource(id = formErrors.dateError.messageId), color = Color.Red)
        }

        Spacer(Modifier.height(8.dp))

        // PASSWORD
        PasswordTextField(
            label = stringResource(id = R.string.password_label),
            value = formDto.password,
            onValueChange = viewModel::onPasswordChange,
            isError = formErrors.passwordError != null,
            errorMessage = formErrors.passwordError?.let { stringResource(id = it.messageId, *it.args.toTypedArray()) } ?: ""
        )

        // CONFIRMAR PASSWORD
        PasswordTextField(
            label = stringResource(id = R.string.password_confirm),
            value = passwordVerification,
            onValueChange = viewModel::onPasswordVerificationChange,
            isError = formErrors.passwordVerifyError != null,
            errorMessage = formErrors.passwordVerifyError?.let { stringResource(id = it.messageId, *it.args.toTypedArray()) } ?: ""
        )

        Spacer(Modifier.height(16.dp))

        // MENSAGEM DE ERRO GERAL (DA API OU VALIDAÇÃO)
        if (globalErrorMessage != null) {
            Text(
                text = globalErrorMessage,
                color = Color.Red,
                modifier = Modifier.padding(bottom = 8.dp)
            )
        }

        Button(
            onClick = onSubmit,
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
}

/**
 * Formulário de Registo contendo todos os campos de entrada e a lógica de submissão.
 *
 * Este componente gere o estado local de todos os campos do formulário (Stateful Component).
 *
 * **Fluxo de Submissão:**
 * 1. Validação síncrona ([validateSignUpForm]).
 * 2. Se válida: Constrói o [CreateProfileDto] e chama a lógica de negócio ([onRegister]).
 * 3. Após sucesso, navega para a Homepage ([Routes.GeralRoutes.HOMEPAGE]).
 *
 * @param navHostController Usado para navegação após sucesso.
 * @param onRegister Callback para executar a lógica de registo.
 */
@Composable
private fun FieldsSignUp(
    navHostController: NavHostController,
    onRegister: (CreateProfileDto, () -> Unit, (String) -> Unit) -> Unit
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
    var isLoading by remember { mutableStateOf(false) }
    var errorMessage by remember { mutableStateOf<String?>(null) }

    // --- ViewModels e Scopes ---
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
        errorMessage = "",
        textFieldModifier = Modifier.testTag(stringResource(id = R.string.tag_email_input))
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
                //errorMessage = "${validationResult.fieldName}: ${validationResult.errorMessage}"
            } else {
                isLoading = true
                errorMessage = null


                onRegister(
                    userProfile,
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
/*
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
/*
@Composable
fun SignUpScreenContentPreview() {
    AMFootballTheme {
        ContentSignUp(
            navController = rememberNavController(),
            onRegister = { _, onSuccess, _ ->
                onSuccess()
            }
        )
    }
}

 */