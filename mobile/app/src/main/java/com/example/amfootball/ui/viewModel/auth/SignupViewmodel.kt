package com.example.amfootball.ui.viewModel.auth

import androidx.navigation.NavHostController
import com.example.amfootball.R
import com.example.amfootball.data.dtos.player.CreateProfileDto
import com.example.amfootball.data.errors.ErrorMessage
import com.example.amfootball.data.errors.formErrors.SignUpFormErrors
import com.example.amfootball.data.network.NetworkConnectivityObserver
import com.example.amfootball.data.services.AuthService
import com.example.amfootball.data.validators.SignUpField
import com.example.amfootball.data.validators.validateSignUpForm
import com.example.amfootball.navigation.objects.Routes
import com.example.amfootball.ui.viewModel.abstracts.FormsViewModel
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.asStateFlow
import java.text.SimpleDateFormat
import java.util.Date
import java.util.Locale
import java.util.TimeZone
import javax.inject.Inject

@HiltViewModel
class SignupViewmodel @Inject constructor(
    private val authService: AuthService,
    private val networkObserver: NetworkConnectivityObserver,
    // Se precisares de outros serviços injeta aqui
) : FormsViewModel<CreateProfileDto, SignUpFormErrors>(
    networkObserver = networkObserver,
    initialData = CreateProfileDto(
        userName = "",
        phone = "",
        height = 0,
        dateOfBirth = "",
        position = 0,
        address = "",
        password = "",
        email = ""
    ),
    initialError = SignUpFormErrors()
) {

    // --- Estados Auxiliares (que não estão diretamente no DTO ou precisam de gestão separada) ---

    // O código do país é gerido à parte e concatenado ao submeter
    private val _countryCode = MutableStateFlow("+351")
    val countryCode = _countryCode.asStateFlow()

    // A verificação da password não vai para a API, serve apenas para validação local
    private val _passwordVerification = MutableStateFlow("")
    val passwordVerification = _passwordVerification.asStateFlow()

    // Guardamos a data em millis para facilitar a validação e o display no DatePicker
    private var dateOfBirthMillis: Long? = null

    // Formatador para a API
    private val apiDateFormatter = SimpleDateFormat("yyyy-MM-dd", Locale.getDefault()).apply {
        timeZone = TimeZone.getTimeZone("UTC")
    }

    // Formatador para Display (podes usar este se quiseres formatar no VM)
    val displayDateFormatter = SimpleDateFormat("dd/MM/yyyy", Locale.getDefault())
    init {
        stopLoading()
    }

    // ============================================================================================
    //  SETTERS (Data Binding)
    // ============================================================================================

    fun onNameChange(name: String) {
        formState.value = formState.value.copy(userName = name)
    }

    fun onEmailChange(email: String) {
        formState.value = formState.value.copy(email = email)
    }

    fun onPhoneChange(phone: String) {
        formState.value = formState.value.copy(phone = phone) // Aqui guardamos apenas o número
    }

    fun onCountryCodeChange(code: String) {
        _countryCode.value = code
    }

    fun onHeightChange(heightStr: String) {
        // Guardamos como string temporariamente na UI se o DTO aceitasse String,
        // mas como é Int, tentamos converter ou guardamos 0 se vazio para evitar crash
        val heightInt = heightStr.toIntOrNull() ?: 0
        formState.value = formState.value.copy(height = heightInt)
    }

    fun onAddressChange(address: String) {
        formState.value = formState.value.copy(address = address)
    }

    fun onPositionChange(positionOrdinal: Int?) {
        if (positionOrdinal != null) {
            formState.value = formState.value.copy(position = positionOrdinal)
        }
    }

    fun onDateChange(millis: Long?) {
        dateOfBirthMillis = millis
        if (millis != null) {
            val formattedDate = apiDateFormatter.format(Date(millis))
            formState.value = formState.value.copy(dateOfBirth = formattedDate)
        } else {
            formState.value = formState.value.copy(dateOfBirth = "")
        }
    }

    fun onPasswordChange(pass: String) {
        formState.value = formState.value.copy(password = pass)
    }

    fun onPasswordVerificationChange(pass: String) {
        _passwordVerification.value = pass
    }

    // ============================================================================================
    //  AÇÕES E VALIDAÇÃO
    // ============================================================================================

    fun onSubmit(navHostController: NavHostController) {
        // Antes de enviar, concatenamos o countryCode ao telemóvel
        val fullPhoneNumber = "${_countryCode.value}${formState.value.phone}"
        // Atualizamos o estado com o número completo apenas para o envio (ou criamos uma cópia local)
        val finalDto = formState.value.copy(phone = fullPhoneNumber)

        // Chamamos a função genérica do Pai
        submitForm(
            onSuccess = {
                navHostController.navigate(Routes.GeralRoutes.HOMEPAGE.route) {
                    popUpTo(navHostController.graph.startDestinationId) { inclusive = true }
                    launchSingleTop = true
                }
            },
            apiCall = {
                // Aqui usamos o finalDto com o numero completo
                authService.registerUser(finalDto)
            }
        )
    }

    override fun validateForm(): Boolean {
        val currentDto = formState.value

        // Usamos a tua função de validação existente
        val validationResult = validateSignUpForm(
            name = currentDto.userName,
            phone = currentDto.phone,
            height = if(currentDto.height == 0) "" else currentDto.height.toString(),
            email = currentDto.email,
            password = currentDto.password,
            passwordVerification = _passwordVerification.value,
            dateOfBirth = dateOfBirthMillis,
            position = currentDto.position
        )

        // Mapear o erro retornado pela tua função `validateSignUpForm` para o objeto `SignUpFormErrors`.
        // Nota: A tua função validateSignUpForm parece retornar apenas UM erro de cada vez (fail-fast).
        // Para uma UX melhor, o ideal seria validar tudo, mas vamos adaptar ao que tens.

        if (!validationResult.isValid) {
            // Criamos uma mensagem de erro genérica baseada no campo que falhou
            if (validationResult.errorMessageId !=null) {
                val errorMsg = ErrorMessage(messageId = validationResult.errorMessageId, args = validationResult.args)
            // Atribuímos o erro ao campo correto
            val errors = when (validationResult.fieldName) {
                SignUpField.NAME.name -> SignUpFormErrors(nameError = errorMsg)
                SignUpField.EMAIL.name -> SignUpFormErrors(emailError = errorMsg)
                SignUpField.PHONE.name -> SignUpFormErrors(phoneError = errorMsg)
                SignUpField.ADDRESS.name -> SignUpFormErrors(addressError = errorMsg)
                SignUpField.HEIGHT.name -> SignUpFormErrors(heightError = errorMsg)
                SignUpField.POSITION.name -> SignUpFormErrors(positionError = errorMsg)
                SignUpField.DATE_OF_BIRTH.name -> SignUpFormErrors(dateError = errorMsg)
                SignUpField.PASSWORD.name -> SignUpFormErrors(passwordError = errorMsg)
                SignUpField.PASSWORD_VERIFICATION.name -> SignUpFormErrors(passwordVerifyError = errorMsg)
                else -> SignUpFormErrors()
            }
            formErrors.value = errors
            }

        } else {
            formErrors.value = SignUpFormErrors() // Limpa erros
        }

        return validationResult.isValid
    }
}