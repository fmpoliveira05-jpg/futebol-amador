package com.example.amfootball.ui.screens.user

import android.Manifest
import android.content.pm.PackageManager
import android.widget.Toast
import androidx.activity.compose.rememberLauncherForActivityResult
import androidx.activity.result.contract.ActivityResultContracts
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.foundation.verticalScroll
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.MyLocation
import androidx.compose.material3.AlertDialog
import androidx.compose.material3.Button
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.getValue
import androidx.compose.runtime.remember
import androidx.compose.runtime.rememberCoroutineScope
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.platform.testTag
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.core.app.ActivityCompat
import androidx.hilt.navigation.compose.hiltViewModel
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import androidx.navigation.NavHostController
import com.example.amfootball.R
import com.example.amfootball.core.utils.GeneralConst
import com.example.amfootball.core.utils.PlayerConst
import com.example.amfootball.core.utils.UserConst
import com.example.amfootball.data.remote.dtos.player.CreateProfileDto
import com.example.amfootball.domains.enums.Position
import com.example.amfootball.domains.errors.formErrors.SignUpFormErrors
import com.example.amfootball.ui.OsmMapView
import com.example.amfootball.ui.components.Loading
import com.example.amfootball.ui.components.inputFields.DatePickerDockedPastLimitedDate
import com.example.amfootball.ui.components.inputFields.EmailTextField
import com.example.amfootball.ui.components.inputFields.LabelSelectBox
import com.example.amfootball.ui.components.inputFields.PasswordTextField
import com.example.amfootball.ui.components.inputFields.PhoneInputWithDynamicCountries
import com.example.amfootball.ui.components.inputFields.TextFieldOutline
import com.example.amfootball.ui.theme.AMFootballTheme
import com.example.amfootball.ui.viewModel.auth.SignupViewmodel
import com.google.android.gms.location.LocationServices

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
    viewModel: SignupViewmodel = hiltViewModel(),
    profileEditMode: Boolean = false

) {
    val uiFormState by viewModel.uiFormState.collectAsStateWithLifecycle()
    val uiErrors by viewModel.uiFormErrors.collectAsStateWithLifecycle()
    val uiState by viewModel.uiState.collectAsStateWithLifecycle()
    val isOnline by viewModel.isOnline.collectAsStateWithLifecycle()
    val context = LocalContext.current
    val scope = rememberCoroutineScope()
    val fusedLocationClient = remember { LocationServices.getFusedLocationProviderClient(context) }
    LaunchedEffect(key1 = Unit) {
        if (profileEditMode) {
            viewModel.onEditMode()
        }
    }
    val countryCode by viewModel.countryCode.collectAsStateWithLifecycle()
    val passwordVerification by viewModel.passwordVerification.collectAsStateWithLifecycle()

    val locationPermissionLauncher = rememberLauncherForActivityResult(
        contract = ActivityResultContracts.RequestMultiplePermissions()
    ) { permissions ->
        val isGranted = permissions[Manifest.permission.ACCESS_FINE_LOCATION] == true ||
                permissions[Manifest.permission.ACCESS_COARSE_LOCATION] == true

        if (isGranted) {
            // SUCESSO: Passamos a bola para o ViewModel
            viewModel.fetchAddressLocation(fusedLocationClient)
        } else {
            Toast.makeText(context, "Permissão necessária para preencher morada", Toast.LENGTH_SHORT).show()
        }
    }

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
        viewModel = viewModel,
        onSubmit = {
            viewModel.onSubmit(
                isEditMode = profileEditMode,
                onDirectSubmit = {
                    viewModel.submitConfirmation(navHostController, profileEditMode)
                }
            )
        },
        submitConfirmation = {
            viewModel.submitConfirmation(navHostController, profileEditMode)
        },
        onRequestLocation = {
            if (ActivityCompat.checkSelfPermission(context, Manifest.permission.ACCESS_FINE_LOCATION) == PackageManager.PERMISSION_GRANTED ||
                ActivityCompat.checkSelfPermission(context, Manifest.permission.ACCESS_COARSE_LOCATION) == PackageManager.PERMISSION_GRANTED
            ) {
                viewModel.fetchAddressLocation(fusedLocationClient)
            } else {
                locationPermissionLauncher.launch(
                    arrayOf(Manifest.permission.ACCESS_FINE_LOCATION, Manifest.permission.ACCESS_COARSE_LOCATION)
                )
            }
        },
        profileEditMode = profileEditMode
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
    modifier: Modifier = Modifier,
    submitConfirmation: () -> Unit,
    onRequestLocation: () -> Unit,
    profileEditMode: Boolean = false
) {
    Column(
        modifier = modifier,
        verticalArrangement = Arrangement.Center,
        horizontalAlignment = Alignment.CenterHorizontally
    ) {
        if (viewModel._foundLat.value != null && viewModel._foundLon.value != null) {

            AlertDialog(
                onDismissRequest = {
                    viewModel._foundLat.value = null
                    viewModel._foundLon.value = null
                },
                title = { Text("Confirmar Localização") },
                text = {
                    OsmMapView(
                        latitude = viewModel._foundLat.value!!,
                        longitude = viewModel._foundLon.value!!,
                        modifier = Modifier
                            .fillMaxWidth()
                            .height(300.dp)
                    )
                },
                confirmButton = {
                    Button(onClick = {
                        viewModel._foundLat.value = null
                        viewModel._foundLon.value = null
                        submitConfirmation()

                    }) {
                        Text("Confirmar")
                    }
                }
            )
        }

        Spacer(Modifier.height(20.dp))

        // NOME
        TextFieldOutline(
            label = stringResource(id = R.string.player_name),
            value = formDto.userName,
            onValueChange = viewModel::onNameChange,
            maxLenght = UserConst.MAX_NAME_LENGTH,
            isRequired = true,
            isError = formErrors.nameError != null,
            errorMessage = formErrors.nameError?.let {
                stringResource(
                    id = it.messageId,
                    *it.args.toTypedArray()
                )
            }
        )

        // EMAIL
        EmailTextField(
            value = formDto.email,
            onValueChange = viewModel::onEmailChange,
            isError = formErrors.emailError != null,
            errorMessage = formErrors.emailError?.let {
                stringResource(
                    id = it.messageId,
                    *it.args.toTypedArray()
                )
            } ?: "",
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
            errorMessage = formErrors.phoneError?.let {
                stringResource(
                    id = it.messageId,
                    *it.args.toTypedArray()
                )
            }
        )

        // MORADA

        Row(
            modifier = Modifier.fillMaxWidth(),
            verticalAlignment = Alignment.CenterVertically,
            horizontalArrangement = Arrangement.spacedBy(8.dp)
        ) {
            Box(modifier = Modifier.weight(1f)) {
                TextFieldOutline(
                    label = stringResource(id = R.string.address),
                    value = formDto.address,
                    maxLenght = GeneralConst.MAX_ADDRESS_LENGTH,
                    onValueChange = viewModel::onAddressChange,
                    isRequired = true,
                    isError = formErrors.addressError != null,
                    errorMessage = formErrors.addressError?.let {
                        stringResource(id = it.messageId, *it.args.toTypedArray())
                    }
                )
            }

            // Lógica do Botão: Mostra Loading ou Mostra Ícone
            if (viewModel.isLocationLoading.value) {
                CircularProgressIndicator(
                    modifier = Modifier.size(24.dp).padding(top = 8.dp),
                    strokeWidth = 2.dp
                )
            } else {
                IconButton(
                    onClick = onRequestLocation,
                    modifier = Modifier.padding(top = 8.dp)
                ) {
                    Icon(
                        imageVector = Icons.Default.MyLocation,
                        contentDescription = "Usar minha localização",
                        tint = MaterialTheme.colorScheme.primary
                    )
                }
            }
        }

        // ALTURA
        TextFieldOutline(
            label = stringResource(id = R.string.player_size),
            value = if (formDto.height == 0) "" else formDto.height.toString(),
            onValueChange = viewModel::onHeightChange,
            isRequired = true,
            maxLenght = PlayerConst.MAX_HEIGHT,
            keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number),
            isError = formErrors.heightError != null,
            errorMessage = formErrors.heightError?.let {
                stringResource(
                    id = it.messageId,
                    *it.args.toTypedArray()
                )
            }
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
        if (formErrors.positionError != null) {
            Text(text = stringResource(id = formErrors.positionError.messageId), color = Color.Red)
        }

        Spacer(Modifier.height(8.dp))

        // DATA DE NASCIMENTO
        Text(text = "${stringResource(R.string.date_of_birthday)}:")
        val displayDate = formDto.dateOfBirth.ifBlank { "" }

        DatePickerDockedPastLimitedDate(
            value = displayDate,
            onDateSelected = viewModel::onDateChange,
            label = stringResource(R.string.date_of_birthday),
            contentDescription = stringResource(R.string.date_of_birthday_description),
            isSingleLine = true,
        )
        if (formErrors.dateError != null) {
            Text(text = stringResource(id = formErrors.dateError.messageId), color = Color.Red)
        }

        Spacer(Modifier.height(8.dp))

        // PASSWORD
        if(!profileEditMode) {
            PasswordTextField(
                label = stringResource(id = R.string.password_label),
                value = formDto.password,
                onValueChange = viewModel::onPasswordChange,
                isError = formErrors.passwordError != null,
                errorMessage = formErrors.passwordError?.let {
                    stringResource(
                        id = it.messageId,
                        *it.args.toTypedArray()
                    )
                } ?: ""
            )

            // CONFIRMAR PASSWORD
            PasswordTextField(
                label = stringResource(id = R.string.password_confirm),
                value = passwordVerification,
                onValueChange = viewModel::onPasswordVerificationChange,
                isError = formErrors.passwordVerifyError != null,
                errorMessage = formErrors.passwordVerifyError?.let {
                    stringResource(
                        id = it.messageId,
                        *it.args.toTypedArray()
                    )
                } ?: ""
            )
        }

        Spacer(Modifier.height(16.dp))

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
@Preview(
    name = "SignUp – PT",
    locale = "pt-rPT",
    showBackground = true,
    showSystemUi = true
)

@Composable
fun SignUpPreview() {
    AMFootballTheme {
        ContentSignUp(
            modifier = Modifier
                .fillMaxSize()
                .padding(16.dp),
            formDto = CreateProfileDto(
                userName = "João Silva",
                email = "joao@email.com",
                phone = "912345678",
                address = "Rua Exemplo 123",
                height = 180,
                position = 1,
                dateOfBirth = "2000-05-10",
                password = "Password123"
            ),
            formErrors = SignUpFormErrors(),
            countryCode = "+351",
            passwordVerification = "Password123",
            isLoading = false,
            globalErrorMessage = null,
            viewModel = ,
            onSubmit = {},
            submitConfirmation = {},
            profileEditMode = false
        )
    }

}
 */