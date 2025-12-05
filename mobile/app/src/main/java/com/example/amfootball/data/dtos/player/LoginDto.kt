package com.example.amfootball.data.dtos.player

/**
 * Objeto de Transferência de Dados (DTO) utilizado para a autenticação (Login) do utilizador.
 *
 * Este objeto encapsula as credenciais necessárias para validar a identidade do utilizador
 * junto do backend ou serviço de autenticação (ex: Firebase Auth, API REST).
 *
 * As propriedades possuem valores por defeito vazios para facilitar a inicialização
 * de estados em formulários de UI (ex: Jetpack Compose State).
 *
 * @property email O endereço de correio eletrónico associado à conta do utilizador.
 * Valor por defeito: String vazia (`""`).
 *
 * @property password A palavra-passe de segurança da conta.
 * Valor por defeito: String vazia (`""`).
 */
data class LoginDto(
    val email: String = "",
    val password: String = ""
)