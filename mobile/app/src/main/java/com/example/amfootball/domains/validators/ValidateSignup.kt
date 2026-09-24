package com.example.amfootball.domains.validators

import android.util.Patterns
import com.example.amfootball.R
import com.example.amfootball.core.utils.GeneralConst
import com.example.amfootball.core.utils.PlayerConst
import com.example.amfootball.core.utils.UserConst
import com.example.amfootball.domains.enums.Position
import java.util.Calendar

/**
 * Um objeto de resultado padrão para a nossa validação.
 *
 * @property isValid Indica se a validação passou (true) ou falhou (false).
 * @property fieldName O nome do campo que falhou (Enum [SignUpField] convertido para String).
 * @property errorMessage A mensagem de erro específica a ser mostrada ao utilizador (opcional).
 */
data class ValidationResult(
    val isValid: Boolean,
    val fieldName: String? = null,
    val errorMessageId: Int? = null,
    val args: List<Any> = emptyList()
)

/**
 * Enumeração para os nomes dos campos do formulário de registo, para evitar erros de digitação.
 */
enum class SignUpField {
    NAME, PHONE, ADDRESS, HEIGHT, EMAIL, PASSWORD, PASSWORD_VERIFICATION, DATE_OF_BIRTH, POSITION
}

/**
 * Função principal que valida todo o formulário de registo.
 *
 * Executa todas as funções de validação individuais e retorna o resultado do primeiro erro que encontrar.
 * Se todas as validações passarem, retorna sucesso.
 *
 * @return O primeiro [ValidationResult] que indica uma falha, ou sucesso se todos os campos forem válidos.
 */
fun validateSignUpForm(
    name: String,
    phone: String,
    height: String,
    email: String,
    address: String,
    password: String,
    passwordVerification: String,
    dateOfBirth: Long?,
    position: Int?
): ValidationResult {
    validateName(name).let { if (!it.isValid) return it }
    validateEmail(email).let { if (!it.isValid) return it }
    validatePhone(phone).let { if (!it.isValid) return it }
    validateAddress(address).let { if (!it.isValid) return it }
    validateHeight(height).let { if (!it.isValid) return it }
    validatePosition(position).let { if (!it.isValid) return it }
    validateDateOfBirth(dateOfBirth).let { if (!it.isValid) return it }
    validatePassword(password).let { if (!it.isValid) return it }
    validatePasswordConfirmation(password, passwordVerification).let { if (!it.isValid) return it }

    // Se todos passaram, retorna sucesso.
    return ValidationResult(isValid = true)
}

// --- Funções de Validação Individuais ---

private fun validateAddress(address: String): ValidationResult {
    if (address.isBlank()) {
        return ValidationResult(
            false,
            SignUpField.ADDRESS.name,
            R.string.error_field_cannot_be_null
        )
    }
    if (address.length < GeneralConst.MIN_ADDRESS_LENGTH) {
        return ValidationResult(
            false,
            SignUpField.ADDRESS.name,
            R.string.error_min_address,
            args = listOf(GeneralConst.MIN_ADDRESS_LENGTH)
        )
    }
    if (address.length > GeneralConst.MAX_ADDRESS_LENGTH) {
        return ValidationResult(
            false,
            SignUpField.ADDRESS.name,
            R.string.error_max_address,
            args = listOf(GeneralConst.MAX_ADDRESS_LENGTH)
        )
    }
    return ValidationResult(true)
}

/**
 * Valida o campo do nome.
 *
 * @param name O valor do nome a ser validado.
 * @return [ValidationResult] indicando sucesso ou a mensagem de erro específica.
 */
private fun validateName(name: String): ValidationResult {
    if (name.isBlank()) {
        return ValidationResult(false, SignUpField.NAME.name, R.string.error_field_cannot_be_null)
    }
    if (name.length < UserConst.MIN_NAME_LENGTH) {
        return ValidationResult(
            false,
            SignUpField.NAME.name,
            R.string.error_min_name_player,
            listOf(UserConst.MIN_NAME_LENGTH)
        )
    }
    if (name.length > UserConst.MAX_NAME_LENGTH) {
        return ValidationResult(
            false,
            SignUpField.NAME.name,
            R.string.error_max_name_player,
            listOf(UserConst.MAX_NAME_LENGTH)
        )
    }
    return ValidationResult(true)
}

/**
 * Valida o campo do email.
 *
 * Verifica se o campo não está vazio e se o formato do email é válido usando o padrão Android.
 *
 * @param email O valor do email a ser validado.
 * @return [ValidationResult] indicando sucesso ou a mensagem de erro específica.
 */
private fun validateEmail(email: String): ValidationResult {
    if (email.isBlank()) {
        return ValidationResult(false, SignUpField.EMAIL.name, R.string.error_field_cannot_be_null)
    }
    if (email.length < UserConst.MIN_EMAIL_LENGTH) {
        return ValidationResult(
            false,
            SignUpField.EMAIL.name,
            R.string.error_min_email_length,
            args = listOf(UserConst.MIN_EMAIL_LENGTH)
        )
    }
    if (email.length > UserConst.MAX_EMAIL_LENGTH) {
        return ValidationResult(
            false,
            SignUpField.EMAIL.name,
            R.string.error_max_email_length,
            listOf(UserConst.MAX_EMAIL_LENGTH)
        )
    }
    if (!Patterns.EMAIL_ADDRESS.matcher(email).matches()) {
        return ValidationResult(false, SignUpField.EMAIL.name, R.string.error_invalid_email)
    }
    return ValidationResult(true)
}

/**
 * Valida o campo da palavra-passe (Password).
 *
 * Verifica vários critérios de segurança: comprimento mínimo (8), letras maiúsculas, minúsculas,
 * números, caracteres especiais e ausência de espaços.
 *
 * @param password A palavra-passe a ser validada.
 * @return [ValidationResult] indicando sucesso ou a mensagem de erro específica de segurança.
 */
private fun validatePassword(password: String): ValidationResult {
    if (password.isBlank()) {
        return ValidationResult(
            false,
            SignUpField.PASSWORD.name,
            R.string.error_field_cannot_be_null
        )
    }
    if (password.length < UserConst.MIN_PASSWORD_LENGTH) {
        return ValidationResult(
            false,
            SignUpField.PASSWORD.name,
            R.string.error_min_password_length,
            listOf(UserConst.MIN_PASSWORD_LENGTH)
        )
    }
    if (!password.any { it.isUpperCase() }) {
        return ValidationResult(
            false,
            SignUpField.PASSWORD.name,
            R.string.error_password_requires_uppercase
        )
    }
    if (!password.any { it.isLowerCase() }) {
        return ValidationResult(
            false,
            SignUpField.PASSWORD.name,
            R.string.error_password_requires_lowercase
        )
    }
    if (!password.any { it.isDigit() }) {
        return ValidationResult(
            false,
            SignUpField.PASSWORD.name,
            R.string.error_password_requires_digit
        )
    }
    if (!password.any { !it.isLetterOrDigit() }) {
        return ValidationResult(
            false,
            SignUpField.PASSWORD.name,
            R.string.error_password_requires_special_char
        )
    }
    if (password.any { it.isWhitespace() }) {
        return ValidationResult(
            false,
            SignUpField.PASSWORD.name,
            R.string.error_password_requires_no_whitespace

        )
    }
    if (password.length > UserConst.MAX_PASSWORD_LENGTH) {
        return ValidationResult(
            false,
            SignUpField.PASSWORD.name,
            R.string.error_max_password_length,
            listOf(UserConst.MAX_PASSWORD_LENGTH)
        )
    }

    return ValidationResult(true)
}

/**
 * Valida o campo de confirmação de palavra-passe.
 *
 * Verifica se o campo não está vazio e se o valor é idêntico à palavra-passe original.
 *
 * @param password A palavra-passe original.
 * @param confirmation O valor da confirmação.
 * @return [ValidationResult] indicando sucesso ou falha por não coincidência.
 */
private fun validatePasswordConfirmation(password: String, confirmation: String): ValidationResult {
    if (confirmation.isBlank()) {
        return ValidationResult(
            false,
            SignUpField.PASSWORD_VERIFICATION.name,
            R.string.error_field_cannot_be_null
        )
    }
    if (password != confirmation) {
        return ValidationResult(
            false,
            SignUpField.PASSWORD_VERIFICATION.name,
            R.string.error_password_mismatch
        )
    }
    return ValidationResult(true)
}

/**
 * Valida o campo do número de telemóvel.
 *
 * Verifica se o campo não está vazio e se corresponde exatamente a 9 dígitos (formato local/nacional).
 *
 * @param phone O valor do telemóvel a ser validado.
 * @return [ValidationResult] indicando sucesso ou a mensagem de erro específica.
 */
private fun validatePhone(phone: String): ValidationResult {
    if (phone.isBlank()) {
        return ValidationResult(false, SignUpField.PHONE.name, R.string.error_field_cannot_be_null)
    }
    if (!phone.matches(Regex("^\\d{${UserConst.SIZE_PHONE_NUMBER}}$"))) {
        return ValidationResult(
            false,
            SignUpField.PHONE.name,
            R.string.error_min_phone_length,
            listOf(UserConst.SIZE_PHONE_NUMBER)
        )
    }
    return ValidationResult(true)
}

/**
 * Valida o campo da altura.
 *
 * Verifica se o campo não está vazio, se é um número inteiro válido e se está dentro do intervalo
 * lógico de 100 cm a 250 cm.
 *
 * @param height O valor da altura (como String) a ser validado.
 * @return [ValidationResult] indicando sucesso ou a mensagem de erro específica.
 */
private fun validateHeight(height: String): ValidationResult {
    if (height.isBlank()) {
        return ValidationResult(false, SignUpField.HEIGHT.name, R.string.error_field_cannot_be_null)
    }
    val heightInt = height.toIntOrNull()
    if (heightInt == null || heightInt !in 100..250) {
        return ValidationResult(
            false,
            SignUpField.HEIGHT.name,
            R.string.error_min_height_max_height,
            listOf(PlayerConst.MIN_HEIGHT, PlayerConst.MAX_HEIGHT)
        )
    }
    return ValidationResult(true)
}

/**
 * Valida o campo da data de nascimento.
 *
 * Verifica se a data foi selecionada (não é nula).
 * Verifica se a idade é maior que 18 anos e menor que 70 anos.
 *
 * @param dateInMillis O valor da data de nascimento em milissegundos (Long?).
 * @return [ValidationResult] indicando sucesso ou falha por campo vazio.
 */
private fun validateDateOfBirth(dateInMillis: Long?): ValidationResult {
    if (dateInMillis == null) {
        return ValidationResult(
            false,
            SignUpField.DATE_OF_BIRTH.name,
            R.string.error_field_cannot_be_null
        )
    }

    val dob = Calendar.getInstance()
    dob.timeInMillis = dateInMillis

    val today = Calendar.getInstance()

    var age = today.get(Calendar.YEAR) - dob.get(Calendar.YEAR)

    if (today.get(Calendar.DAY_OF_YEAR) < dob.get(Calendar.DAY_OF_YEAR)) {
        age--
    }
    if (age < 18) {
        return ValidationResult(
            false,
            SignUpField.DATE_OF_BIRTH.name,
            R.string.error_min_age,
            listOf(UserConst.MIN_AGE)
        )
    }

    if (age >= 70) {
        return ValidationResult(
            false,
            SignUpField.DATE_OF_BIRTH.name,
            R.string.error_max_age,
            listOf(UserConst.MAX_AGE)
        )
    }

    return ValidationResult(true)
}

/**
 * Valida o campo da posição de jogo.
 *
 * Verifica se uma posição foi selecionada e se o valor inteiro corresponde a um ordinal válido
 * dentro do Enum [Position].
 *
 * @param position O índice ordinal da posição selecionada (Int?).
 * @return [ValidationResult] indicando sucesso ou falha por campo vazio ou posição inválida.
 */
private fun validatePosition(position: Int?): ValidationResult {
    if (position == null) {
        return ValidationResult(
            false,
            SignUpField.POSITION.name,
            R.string.error_field_cannot_be_null
        )
    }
    val isValidPosition = position in Position.values().indices
    if (!isValidPosition) {
        return ValidationResult(
            false,
            SignUpField.POSITION.name,
            R.string.error_field_cannot_be_null
        )
    }
    return ValidationResult(true)
}