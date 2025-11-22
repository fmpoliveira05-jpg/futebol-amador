package com.example.amfootball.data.validators


import android.util.Patterns
import com.example.amfootball.data.enums.Position

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
    val errorMessage: String? = null
)

/**
 * Enumeração para os nomes dos campos do formulário de registo, para evitar erros de digitação.
 */
enum class SignUpField {
    NAME, PHONE, HEIGHT, EMAIL, PASSWORD, PASSWORD_VERIFICATION, DATE_OF_BIRTH, POSITION
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
/**
 * Valida o campo do nome.
 *
 * @param name O valor do nome a ser validado.
 * @return [ValidationResult] indicando sucesso ou a mensagem de erro específica.
 */
private fun validateName(name: String): ValidationResult {
    if (name.isBlank()) {
        return ValidationResult(false, SignUpField.NAME.name, "O nome não pode estar vazio.")
    }
    if (name.length < 3) {
        return ValidationResult(false, SignUpField.NAME.name, "O nome deve ter pelo menos 3 caracteres.")
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
        return ValidationResult(false, SignUpField.EMAIL.name, "O email não pode estar vazio.")
    }
    if (!Patterns.EMAIL_ADDRESS.matcher(email).matches()) {
        return ValidationResult(false, SignUpField.EMAIL.name, "O formato do email é inválido.")
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
        return ValidationResult(false, SignUpField.PASSWORD_VERIFICATION.name, "Por favor, confirme a password.")
    }
    if (password != confirmation) {
        return ValidationResult(false, SignUpField.PASSWORD_VERIFICATION.name, "As passwords não coincidem.")
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
        return ValidationResult(false, SignUpField.PHONE.name, "O telemóvel não pode estar vazio.")
    }
    if (!phone.matches(Regex("^\\d{9}$"))) {
        return ValidationResult(false, SignUpField.PHONE.name, "O telemóvel deve ter 9 dígitos.")
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
        return ValidationResult(false, SignUpField.HEIGHT.name, "A altura não pode estar vazia.")
    }
    val heightInt = height.toIntOrNull()
    if (heightInt == null || heightInt !in 100..250) {
        return ValidationResult(false, SignUpField.HEIGHT.name, "Altura inválida (deve ser entre 100 e 250 cm).")
    }
    return ValidationResult(true)
}

/**
 * Valida o campo da data de nascimento.
 *
 * Verifica se a data foi selecionada (não é nula).
 *
 * @param dateInMillis O valor da data de nascimento em milissegundos (Long?).
 * @return [ValidationResult] indicando sucesso ou falha por campo vazio.
 */
private fun validateDateOfBirth(dateInMillis: Long?): ValidationResult {
    if (dateInMillis == null) {
        return ValidationResult(false, SignUpField.DATE_OF_BIRTH.name, "Por favor, selecione a data de nascimento.")
    }
    // TODO: Validar se o user é maior que a idade definida como idade minima (acho que foram 18 anos)
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
        return ValidationResult(false, SignUpField.POSITION.name, "Por favor, selecione a sua posição.")
    }
    // Verifica se o Int é um ordinal válido no nosso enum
    val isValidPosition = position in Position.values().indices
    if (!isValidPosition) {
        return ValidationResult(false, SignUpField.POSITION.name, "Posição inválida.")
    }
    return ValidationResult(true)
}