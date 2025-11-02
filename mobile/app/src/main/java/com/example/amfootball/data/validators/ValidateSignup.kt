package com.example.amfootball.data.validators


import android.util.Patterns
import com.example.amfootball.data.enums.Position

/**
 * Um objeto de resultado para a nossa validação.
 * @param isValid Se a validação passou.
 * @param fieldName O nome do campo que falhou (opcional).
 * @param errorMessage A mensagem de erro a ser mostrada (opcional).
 */
data class ValidationResult(
    val isValid: Boolean,
    val fieldName: String? = null,
    val errorMessage: String? = null
)

/**
 * Enumeração para os nomes dos campos, para evitar erros de digitação.
 */
enum class SignUpField {
    NAME, PHONE, HEIGHT, EMAIL, PASSWORD, PASSWORD_VERIFICATION, DATE_OF_BIRTH, POSITION
}

/**
 * Função principal que valida todo o formulário de registo.
 * Retorna o primeiro erro que encontrar.
 */
fun validateSignUpForm(
    name: String,
    phone: String,
    height: String,
    email: String,
    password: String,
    passwordVerification: String,
    dateOfBirth: Long?,
    position: Int?
): ValidationResult {
    validateName(name).let { if (!it.isValid) return it }
    validatePhone(phone).let { if (!it.isValid) return it }
    validateHeight(height).let { if (!it.isValid) return it }
    //acrescentar aqui o validate morada
    validateEmail(email).let { if (!it.isValid) return it }
    validatePosition(position).let { if (!it.isValid) return it }
    validateDateOfBirth(dateOfBirth).let { if (!it.isValid) return it }
    validatePassword(password).let { if (!it.isValid) return it }
    validatePasswordConfirmation(password, passwordVerification).let { if (!it.isValid) return it }

    // Se todos passaram, retorna sucesso.
    return ValidationResult(isValid = true)
}

// --- Funções de Validação Individuais ---

private fun validateName(name: String): ValidationResult {
    if (name.isBlank()) {
        return ValidationResult(false, SignUpField.NAME.name, "O nome não pode estar vazio.")
    }
    if (name.length < 3) {
        return ValidationResult(false, SignUpField.NAME.name, "O nome deve ter pelo menos 3 caracteres.")
    }
    return ValidationResult(true)
}

private fun validateEmail(email: String): ValidationResult {
    if (email.isBlank()) {
        return ValidationResult(false, SignUpField.EMAIL.name, "O email não pode estar vazio.")
    }
    if (!Patterns.EMAIL_ADDRESS.matcher(email).matches()) {
        return ValidationResult(false, SignUpField.EMAIL.name, "O formato do email é inválido.")
    }
    return ValidationResult(true)
}

private fun validatePassword(password: String): ValidationResult {
    if (password.isBlank()) {
        return ValidationResult(false, SignUpField.PASSWORD.name, "A password não pode estar vazia.")
    }
    if (password.length < 8) {
        return ValidationResult(false, SignUpField.PASSWORD.name, "A password deve ter pelo menos 6 caracteres.")
    }
    if (!password.any { it.isUpperCase() }) {
        return ValidationResult(false, SignUpField.PASSWORD.name, "A password deve conter pelo menos uma letra maiúscula.")
    }
    if (!password.any { it.isLowerCase() }) {
        return ValidationResult(false, SignUpField.PASSWORD.name, "A password deve conter pelo menos uma letra minúscula.")
    }
    if (!password.any { it.isDigit() }) {
        return ValidationResult(false, SignUpField.PASSWORD.name, "A password deve conter pelo menos um número.")
    }
    if (!password.any { !it.isLetterOrDigit() }) {
        return ValidationResult(false, SignUpField.PASSWORD.name, "A password deve conter pelo menos um caractere especial.")
    }
    if (password.any { it.isWhitespace() }) {
        return ValidationResult(
            false,
            SignUpField.PASSWORD.name,
            "A password não pode conter espaços em branco."
        )
    }
    if (password.length > 100) {
        return ValidationResult(
            false,
            SignUpField.PASSWORD.name,
            "A password não pode ter mais de 100 caracteres."
        )
    }

    return ValidationResult(true)
}

private fun validatePasswordConfirmation(password: String, confirmation: String): ValidationResult {
    if (confirmation.isBlank()) {
        return ValidationResult(false, SignUpField.PASSWORD_VERIFICATION.name, "Por favor, confirme a password.")
    }
    if (password != confirmation) {
        return ValidationResult(false, SignUpField.PASSWORD_VERIFICATION.name, "As passwords não coincidem.")
    }
    return ValidationResult(true)
}

private fun validatePhone(phone: String): ValidationResult {
    if (phone.isBlank()) {
        return ValidationResult(false, SignUpField.PHONE.name, "O telemóvel não pode estar vazio.")
    }
    if (!phone.matches(Regex("^\\d{9}$"))) {
        return ValidationResult(false, SignUpField.PHONE.name, "O telemóvel deve ter 9 dígitos.")
    }
    return ValidationResult(true)
}

private fun validateHeight(height: String): ValidationResult {
    if (height.isBlank()) {
        return ValidationResult(false, SignUpField.HEIGHT.name, "A altura não pode estar vazia.")
    }
    val heightInt = height.toIntOrNull()
    if (heightInt == null || heightInt !in 100..250) {
        return ValidationResult(false, SignUpField.HEIGHT.name, "Altura inválida (deve ser entre 100 e 250 cm).")
    }
    return ValidationResult(true)
}

private fun validateDateOfBirth(dateInMillis: Long?): ValidationResult {
    if (dateInMillis == null) {
        return ValidationResult(false, SignUpField.DATE_OF_BIRTH.name, "Por favor, selecione a data de nascimento.")
    }
    // TODO: Validar se o user é maior que a idade definida como idade minima (acho que foram 18 anos)
    return ValidationResult(true)
}

private fun validatePosition(position: Int?): ValidationResult {
    if (position == null) {
        return ValidationResult(false, SignUpField.POSITION.name, "Por favor, selecione a sua posição.")
    }
    // Verifica se o Int é um ordinal válido no nosso enum
    val isValidPosition = position in Position.values().indices
    if (!isValidPosition) {
        return ValidationResult(false, SignUpField.POSITION.name, "Posição inválida.")
    }
    return ValidationResult(true)
}