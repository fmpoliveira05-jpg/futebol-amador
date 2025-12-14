package com.example.amfootball.ui.viewModel.auth

import androidx.compose.runtime.mutableStateOf
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
    private val PlayerService: PlayerService,
    private val osmService: OpenStreetMapService,
    private val networkObserver: NetworkConnectivityObserver,
    private val sessionManager: SessionManager,

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

    private val _countryCode = MutableStateFlow("+351")
    val countryCode = _countryCode.asStateFlow()

    private val _passwordVerification = MutableStateFlow("")
    val passwordVerification = _passwordVerification.asStateFlow()

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
        val userProfile = sessionManager.getUserProfile()
        if (userProfile == null) return

        onNameChange(userProfile.name)
        onEmailChange(userProfile.email ?: "")

        val phoneFull = userProfile.phoneNumber ?: ""
        onPhoneChange(phoneFull.replace("+351", ""))

        onHeightChange(userProfile.height.toString())
        onAddressChange(userProfile.address)
        onPositionChange(userProfile.position.ordinal)
        onPasswordChange("Teste123.")
        onPasswordVerificationChange("Teste123.")

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
            val finalDto = formState.value.copy(phone = fullPhoneNumber)

            if (!profileEditMode) {
                authService.registerUser(finalDto)
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
            formErrors.value = SignUpFormErrors() // Limpa erros
        }

        return validationResult.isValid
    }
}