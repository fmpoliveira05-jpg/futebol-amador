package com.example.amfootball.ui.screens.Chat

import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.ArrowBack
import androidx.compose.material.icons.filled.MoreVert
import androidx.compose.material.icons.filled.Send
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Surface
import androidx.compose.material3.Text
import androidx.compose.material3.TopAppBar
import androidx.compose.material3.TopAppBarDefaults
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp

// --- Modelo de Dados Simples para a Mensagem ---

data class Message(
    val id: String,
    val text: String,
    val isSentByMe: Boolean,
    val timestamp: String
)

// --- Composable Principal da Tela de Chat ---

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun ChatScreen() {
    // Lista de mensagens de exemplo (mock)
    val mockMessages = listOf(
        Message("1", "Olá! Tudo bem?", isSentByMe = false, "10:00"),
        Message("2", "Tudo ótimo! E com você?", isSentByMe = true, "10:01"),
        Message("3", "Estou bem também, obrigado por perguntar.", isSentByMe = false, "10:01"),
        Message("4", "Você viu o novo layout do app? Achei incrível!", isSentByMe = false, "10:02"),
        Message("5", "Vi sim! O time de design fez um trabalho excelente.", isSentByMe = true, "10:03"),
        Message("6", "Com certeza. A navegação está muito mais fluida.", isSentByMe = true, "10:03"),
        Message("7", "Verdade!", isSentByMe = false, "10:04")
    )

    // Estado para armazenar o texto digitado
    var messageText by remember { mutableStateOf("") }

    Scaffold(
        topBar = {
            ChatTopBar(contactName = "Jane Doe") {
                // Ação para o botão de voltar (ex: fechar a tela)
            }
        },
        content = { paddingValues ->
            Column(
                modifier = Modifier
                    .fillMaxSize()
                    .padding(paddingValues) // Aplica o padding do Scaffold
            ) {
                // --- 1. Lista de Mensagens ---
                LazyColumn(
                    modifier = Modifier
                        .weight(1f) // Ocupa todo o espaço disponível
                        .padding(horizontal = 16.dp),
                    reverseLayout = false // Opcional: true se quiser começar do fim
                ) {
                    items(mockMessages) { message ->
                        MessageBubble(message = message)
                    }
                }

                // --- 2. Campo de Entrada de Mensagem ---
                MessageInput(
                    message = messageText,
                    onMessageChange = { messageText = it },
                    onSendClick = {
                        // Lógica para enviar a mensagem (aqui apenas limpa o campo)
                        if (messageText.isNotBlank()) {
                            // Adicionar a nova mensagem à lista (em um app real)
                            messageText = ""
                        }
                    }
                )
            }
        }
    )
}

// --- Componentes da Tela de Chat ---

/**
 * Barra superior com nome, foto (placeholder) e ações.
 */
@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun ChatTopBar(contactName: String, onBackClick: () -> Unit) {
    TopAppBar(
        title = {
            Row(verticalAlignment = Alignment.CenterVertically) {
                // Placeholder para a foto do perfil
                Surface(
                    shape = CircleShape,
                    color = MaterialTheme.colorScheme.secondary,
                    modifier = Modifier.size(40.dp)
                ) {
                    // Você pode usar uma imagem real aqui com AsyncImage (Coil)
                    Box(contentAlignment = Alignment.Center) {
                        Text(
                            text = contactName.first().toString(),
                            color = MaterialTheme.colorScheme.onSecondary,
                            fontWeight = FontWeight.Bold
                        )
                    }
                }

                Spacer(modifier = Modifier.width(12.dp))

                Column {
                    Text(
                        text = contactName,
                        fontWeight = FontWeight.Bold,
                        fontSize = 18.sp
                    )
                    Text(
                        text = "Online", // Status
                        fontSize = 12.sp,
                        color = MaterialTheme.colorScheme.onSurfaceVariant
                    )
                }
            }
        },
        navigationIcon = {
            IconButton(onClick = onBackClick) {
                Icon(Icons.Default.ArrowBack, contentDescription = "Voltar")
            }
        },
        actions = {
            IconButton(onClick = { /* TODO: Ação de menu */ }) {
                Icon(Icons.Default.MoreVert, contentDescription = "Mais opções")
            }
        },
        colors = TopAppBarDefaults.topAppBarColors(
            containerColor = MaterialTheme.colorScheme.surface
        )
    )
}

/**
 * Balão de mensagem individual (distingue enviadas e recebidas).
 */
@Composable
fun MessageBubble(message: Message) {
    val alignment = if (message.isSentByMe) Alignment.CenterEnd else Alignment.CenterStart
    val bubbleColor = if (message.isSentByMe) MaterialTheme.colorScheme.primaryContainer else MaterialTheme.colorScheme.surfaceVariant
    val textColor = if (message.isSentByMe) MaterialTheme.colorScheme.onPrimaryContainer else MaterialTheme.colorScheme.onSurfaceVariant
    val bubbleShape = if (message.isSentByMe) {
        RoundedCornerShape(20.dp, 4.dp, 20.dp, 20.dp)
    } else {
        RoundedCornerShape(4.dp, 20.dp, 20.dp, 20.dp)
    }

    Box(
        modifier = Modifier
            .fillMaxWidth()
            .padding(vertical = 4.dp),
        contentAlignment = alignment
    ) {
        Surface(
            shape = bubbleShape,
            color = bubbleColor,
            tonalElevation = 1.dp
        ) {
            Text(
                text = message.text,
                color = textColor,
                modifier = Modifier.padding(horizontal = 16.dp, vertical = 10.dp)
            )
        }
    }
}

/**
 * Campo de texto e botão de enviar na parte inferior.
 */
@Composable
fun MessageInput(
    message: String,
    onMessageChange: (String) -> Unit,
    onSendClick: () -> Unit
) {
    Surface(
        tonalElevation = 2.dp, // Adiciona uma leve sombra/elevação
    ) {
        Row(
            modifier = Modifier
                .fillMaxWidth()
                .padding(horizontal = 8.dp, vertical = 8.dp),
            verticalAlignment = Alignment.CenterVertically
        ) {
            OutlinedTextField(
                value = message,
                onValueChange = onMessageChange,
                placeholder = { Text("Digite uma mensagem...") },
                modifier = Modifier.weight(1f),
                shape = RoundedCornerShape(24.dp)
            )

            Spacer(modifier = Modifier.width(8.dp))

            IconButton(
                onClick = onSendClick,
                enabled = message.isNotBlank(),
                modifier = Modifier
                    .size(48.dp)
                    .clip(CircleShape)
            ) {
                Icon(
                    Icons.Default.Send,
                    contentDescription = "Enviar mensagem",
                    tint = MaterialTheme.colorScheme.primary
                )
            }
        }
    }
}

// --- Preview para o Android Studio ---

@Preview(showBackground = true)
@Composable
fun ChatScreenPreview() {
    // Você pode envolver com seu tema do app se tiver um
    // Ex: SeuAppTheme { ... }
    ChatScreen()
}
