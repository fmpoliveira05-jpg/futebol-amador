package com.example.amfootball.ui.components.inputFields

import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.testTag
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.text.input.KeyboardType
import com.example.amfootball.R
import com.example.amfootball.utils.UserConst

/**
 * Componente de campo de input especializado para Endereços de Email.
 *
 * Configura o [TextFieldOutline] para usar o [KeyboardType.Email] (que otimiza o teclado
 * para incluir o símbolo '@') e aplica o limite de comprimento definido em [UserConst.MAX_EMAIL_LENGTH].
 *
 * @param value O endereço de email atual (String) a ser exibido no campo.
 * @param onValueChange Callback chamado quando o valor do email muda.
 * @param isRequired Booleano que indica se o campo é obrigatório (padrão: true).
 */
@Composable
fun EmailTextField(
    value: String,
    isError: Boolean,
    errorMessage: String?,
    onValueChange: (String) -> Unit,
    isRequired: Boolean = true,
    modifier: Modifier = Modifier,
    textFieldModifier: Modifier = Modifier
) {
    TextFieldOutline(
        label = stringResource(id = R.string.email_field),
        value = value,
        onValueChange = onValueChange,
        isRequired = isRequired,
        maxLenght = UserConst.MAX_EMAIL_LENGTH,
        isError = isError,
        errorMessage = errorMessage,
        keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Email),
        modifier = modifier,
        textFieldModifier = textFieldModifier
    )
}

/**
 * Componente de campo de input especializado para Passwords.
 *
 * Configura o [TextFieldOutline] para usar o [KeyboardType.Password], que:
 * 1. Ativa o mascaramento visual do texto (pontos).
 * 2. Adiciona o [trailingIcon] (ícone de olho) para alternar a visibilidade da password.
 *
 * @param label O rótulo a ser exibido no campo (padrão: "Password").
 * @param value A password atual (String).
 * @param onValueChange Callback chamado quando o valor da password muda.
 * @param isRequired Booleano que indica se o campo é obrigatório (padrão: true).
 */
@Composable
fun PasswordTextField(
    label: String,
    value: String,
    isError: Boolean,
    errorMessage: String?,
    onValueChange: (String) -> Unit,
    isRequired: Boolean = true,
    textFieldModifier: Modifier = Modifier
) {
    TextFieldOutline(
        label = label,
        value = value,
        onValueChange = onValueChange,
        isRequired = isRequired,
        isError = isError,
        errorMessage = errorMessage,
        keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Password),
        textFieldModifier = textFieldModifier
    )
}