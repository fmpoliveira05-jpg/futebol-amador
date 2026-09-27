package com.example.amfootball.ui.viewModel.auth

import android.annotation.SuppressLint
import android.content.Context
import android.location.Geocoder
import android.location.Location
import androidx.compose.runtime.mutableStateOf
import androidx.lifecycle.viewModelScope
import androidx.navigation.NavHostController
import com.example.amfootball.R
import com.example.amfootball.data.NetworkConnectivityObserver
import com.example.amfootball.data.interfaces.services.OpenStreetMapService
import com.example.amfootball.data.local.SessionManager
import com.example.amfootball.data.remote.dtos.player.CreateProfileDto
import com.example.amfootball.data.remote.dtos.player.PlayerProfileDto
import com.example.amfootball.data.remote.services.AuthService
import com.example.amfootball.data.remote.services.PlayerService
import com.example.amfootball.domains.errors.ErrorMessage
import com.example.amfootball.domains.errors.formErrors.SignUpFormErrors
import com.example.amfootball.domains.validators.SignUpField
import com.example.amfootball.domains.validators.validateSignUpForm
import com.example.amfootball.ui.navigation.objects.Routes
import com.example.amfootball.ui.viewModel.abstracts.FormsViewModel
import com.google.android.gms.location.FusedLocationProviderClient
import com.google.android.gms.location.Priority
import com.google.android.gms.tasks.CancellationTokenSource
import dagger.hilt.android.lifecycle.HiltViewModel
import dagger.hilt.android.qualifiers.ApplicationContext
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch
import kotlinx.coroutines.suspendCancellableCoroutine
import kotlinx.coroutines.withContext
import java.text.SimpleDateFormat
import java.util.Date
import java.util.Locale
import java.util.TimeZone
import javax.inject.Inject
import kotlin.coroutines.resumeWithException

@HiltViewModel
class SignupViewmodel @Inject constructor(
    private val authService: AuthService,
    private val PlayerService: PlayerService,
    private val osmService: OpenStreetMapService,
    private val networkObserver: NetworkConnectivityObserver,
    private val sessionManager: SessionManager,
    @ApplicationContext private val context: Context

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

    /** RGPD: aceitação da Política de Privacidade (só no registo, não na edição do perfil). */
    private val _aceitaPolitica = MutableStateFlow(false)
    val aceitaPolitica = _aceitaPolitica.asStateFlow()

    private val _erroPolitica = MutableStateFlow(false)
    val erroPolitica = _erroPolitica.asStateFlow()

    private var modoEdicao = false

    fun onAceitaPoliticaChange(aceita: Boolean) {
        _aceitaPolitica.value = aceita
        if (aceita) _erroPolitica.value = false
    }

    private val _countryCode = MutableStateFlow("+351")
    val countryCode = _countryCode.asStateFlow()

    private val _passwordVerification = MutableStateFlow("")
    val passwordVerification = _passwordVerification.asStateFlow()

    var isLocationLoading = mutableStateOf(false)

    var _foundLat = mutableStateOf<Double?>(null)
    var _foundLon = mutableStateOf<Double?>(null)


    private var dateOfBirthMillis: Long? = null

    private val apiDateFormatter = SimpleDateFormat("yyyy-MM-dd", Locale.getDefault()).apply {
        timeZone = TimeZone.getTimeZone("UTC")
    }

    val displayDateFormatter = SimpleDateFormat("dd/MM/yyyy", Locale.getDefault())

    init {
        stopLoading()
    }

    fun onNameChange(name: String) {
        formState.value = formState.value.copy(userName = name)
    }

    fun onEmailChange(email: String) {
        formState.value = formState.value.copy(email = email)
    }

    fun onPhoneChange(phone: String) {
        formState.value = formState.value.copy(phone = phone)
    }

    fun onCountryCodeChange(code: String) {
        _countryCode.value = code
    }

    fun onHeightChange(heightStr: String) {
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

    fun onSubmit(isEditMode: Boolean = false, onDirectSubmit: () -> Unit = {}) {
        if (!validateForm()) return

        if (isEditMode && addresDidntChanged()) {
            onDirectSubmit()
            return
        }

        launchDataLoad {
            val addressQuery = "${formState.value.address}, Portugal"
            val results = osmService.verifyAddress(address = addressQuery)

            if (results.isEmpty()) {
                val errorMsg = ErrorMessage(messageId = R.string.error_address_not_found)
                formErrors.value = formErrors.value.copy(
                    addressError = errorMsg
                )
                return@launchDataLoad
            }
            _foundLat.value = results[0].lat.toDoubleOrNull()
            _foundLon.value = results[0].lon.toDoubleOrNull()
        }
    }

    private fun addresDidntChanged() : Boolean{
        val userProfile = sessionManager.getUserProfile()
        return userProfile?.address == formState.value.address
    }

    fun onEditMode() {
        modoEdicao = true
        val userProfile = sessionManager.getUserProfile()
        if (userProfile == null) return

        onNameChange(userProfile.name)
        onEmailChange(userProfile.email ?: "")

        val phoneFull = userProfile.phoneNumber ?: ""
        onPhoneChange(phoneFull.replace("+351", ""))

        onHeightChange(userProfile.height.toString())
        onAddressChange(userProfile.address)
        onPositionChange(userProfile.position.ordinal)
        // Valor fictício só para passar a validação do formulário: a edição do perfil não envia a
        // palavra-passe.
        onPasswordChange("Ficticia#Edicao1")
        onPasswordVerificationChange("Ficticia#Edicao1")

        val dateMillis = userProfile.dateOfBirth?.let { dateStr ->
            try {
                apiDateFormatter.parse(dateStr)?.time
            } catch (e: Exception) {
                e.printStackTrace()
                null
            }
        }
        onDateChange(dateMillis)
    }


    fun submitConfirmation(navHostController: NavHostController, profileEditMode: Boolean) {
        launchDataLoad {
            val fullPhoneNumber = "${_countryCode.value}${formState.value.phone}"
            val finalDto = formState.value.copy(
                phone = fullPhoneNumber,
                aceitaPoliticaPrivacidade = _aceitaPolitica.value,
                versaoPoliticaPrivacidade = com.example.amfootball.ui.screens.privacidade.VERSAO_POLITICA_PRIVACIDADE,
            )

            if (!profileEditMode) {
                val comSessao = authService.registerUser(finalDto)
                if (!comSessao) {
                    // É preciso confirmar o e-mail antes de entrar.
                    updateToast(R.string.toast_confirm_email)
                    navHostController.navigate(Routes.UserRoutes.LOGIN.route) {
                        popUpTo(navHostController.graph.startDestinationId) { inclusive = false }
                        launchSingleTop = true
                    }
                    return@launchDataLoad
                }
            } else {
                val currentProfile = sessionManager.getUserProfile()

                PlayerService.updatePlayerProfile(
                    playerProfile = PlayerProfileDto(
                        name = finalDto.userName,
                        email = finalDto.email,
                        phoneNumber = finalDto.phone,
                        dateOfBirth = finalDto.dateOfBirth,
                        height = finalDto.height,
                        address = finalDto.address,
                        positionRaw = finalDto.position,
                        loginResponseDto = currentProfile?.loginResponseDto,
                        icon = currentProfile?.icon,
                        team = currentProfile?.team,
                        idTeam = currentProfile?.idTeam ?: currentProfile?.effectiveTeamId,
                        isAdmin = currentProfile?.isAdmin ?: false,
                        playerId = currentProfile?.loginResponseDto?.localId,
                        phone = finalDto.phone
                    )
                )
                updateToast(R.string.toast_playerProfile_edited)
            }

            navHostController.navigate(Routes.GeralRoutes.HOMEPAGE.route) {
                popUpTo(navHostController.graph.startDestinationId) { inclusive = true }
                launchSingleTop = true
            }
        }
    }
    override fun validateForm(): Boolean {
        val currentDto = formState.value

        val validationResult = validateSignUpForm(
            name = currentDto.userName,
            phone = currentDto.phone,
            height = if (currentDto.height == 0) "" else currentDto.height.toString(),
            email = currentDto.email,
            password = currentDto.password,
            passwordVerification = _passwordVerification.value,
            dateOfBirth = dateOfBirthMillis,
            position = currentDto.position,
            address = currentDto.address
        )
        if (!validationResult.isValid) {
            if (validationResult.errorMessageId != null) {
                val errorMsg = ErrorMessage(
                    messageId = validationResult.errorMessageId,
                    args = validationResult.args
                )
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
            formErrors.value = SignUpFormErrors()
        }

        if (validationResult.isValid && !modoEdicao && !_aceitaPolitica.value) {
            _erroPolitica.value = true
            return false
        }

        return validationResult.isValid
    }

    /**
     * Função que gere todo o processo: Pede localização GPS -> Converte em Morada -> Atualiza UI.
     * Recebe o client de localização como parâmetro (vem da UI).
     */
    fun fetchAddressLocation(fusedLocationClient: FusedLocationProviderClient) {
        viewModelScope.launch {
            isLocationLoading.value = true
            try {
                val location = getLastLocation(fusedLocationClient)

                if (location != null) {
                    val addressText = getAddressFromCoordinates(location.latitude, location.longitude)

                    if (addressText != null) {
                        onAddressChange(addressText)
                    } else {
                    }
                }
            } catch (e: Exception) {
                e.printStackTrace()
            } finally {
                isLocationLoading.value = false
            }
        }
    }

    /**
     * Função suspensa auxiliar para converter a API de Callbacks do Google numa Coroutine limpa.
     */
    @SuppressLint("MissingPermission")
    private suspend fun getLastLocation(client: FusedLocationProviderClient): Location? {
        return suspendCancellableCoroutine { continuation ->
            val cancellationTokenSource = CancellationTokenSource()

            // Precisão ao nível do bairro/cidade: chega para a morada e só exige a localização aproximada.
            client.getCurrentLocation(Priority.PRIORITY_BALANCED_POWER_ACCURACY, cancellationTokenSource.token)
                .addOnSuccessListener { location ->
                    if (continuation.isActive) continuation.resume(
                        location,
                        onCancellation = {
                            if (continuation.isActive) continuation.cancel()
                        }
                    )
                }
                .addOnFailureListener { e ->
                    if (continuation.isActive) continuation.resumeWithException(e)
                }
                .addOnCanceledListener {
                    if (continuation.isActive) continuation.cancel()
                }

            continuation.invokeOnCancellation {
                cancellationTokenSource.cancel()
            }
        }
    }

    /**
     * Lógica de Geocoder isolada
     */
    private suspend fun getAddressFromCoordinates(lat: Double, lon: Double): String? {
        return withContext(Dispatchers.IO) {
            try {
                val geocoder = Geocoder(context, Locale.getDefault())
                @Suppress("DEPRECATION")
                val addresses = geocoder.getFromLocation(lat, lon, 1)

                if (!addresses.isNullOrEmpty()) {
                    val addressObj = addresses[0]
                    val street = addressObj.thoroughfare ?: ""
                    val number = addressObj.subThoroughfare ?: ""
                    val city = addressObj.locality ?: addressObj.subAdminArea ?: ""

                    if (street.isNotBlank()) "$street $number, $city".trim() else addressObj.getAddressLine(0)
                } else {
                    null
                }
            } catch (e: Exception) {
                null
            }
        }
    }
}