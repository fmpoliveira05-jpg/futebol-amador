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
import androidx.compose.material.icons.automirrored.filled.Send
import androidx.compose.material.icons.filled.MoreVert
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
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.platform.testTag
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.hilt.navigation.compose.hiltViewModel
import com.example.amfootball.R
import com.example.amfootball.data.dtos.chat.MessageDto
import com.example.amfootball.ui.viewModel.chat.ChatViewModel

/**
 * Ecrã de conversa individual (Single Chat Screen).
 *
 * Responsável por exibir o histórico de mensagens e permitir o envio de novas mensagens
 * em tempo real.
 *
 * **Fluxo de Dados:**
 * 1. O `viewModel` carrega as mensagens (`listenForMessages`) assim que o `chatRoomId` é definido.
 * 2. As mensagens são observadas via `collectAsState` e renderizadas numa `LazyColumn`.
 * 3. Novas mensagens digitadas no `MessageInput` são enviadas através do `viewModel.sendMessage`.
 *
 * @param chatViewModel O ViewModel que gere a lógica de chat e conexão ao Firebase.
 */
@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun ChatScreen(
    chatViewModel: ChatViewModel = hiltViewModel()
) {
    var messageText by remember { mutableStateOf("") }
    val roomName by chatViewModel.roomName.collectAsState()
    Scaffold(
        topBar = {
            ChatTopBar(contactName = roomName)
        },
        content = { paddingValues ->
            Column(
                modifier = Modifier
                    .fillMaxSize()
                    .padding(paddingValues)
            ) {
                LaunchedEffect(key1 = chatViewModel.chatRoomId) {
                    chatViewModel.listenForMessages()
                }

                val messages by chatViewModel.messages.collectAsState()

                LazyColumn(
                    modifier = Modifier
                        .weight(1f)
                        .padding(horizontal = 16.dp),
                    reverseLayout = false
                ) {
                    items(messages) { message ->
                        MessageBubble(
                            message = message,
                            isSentByMe = chatViewModel.isSentByMe(message)
                        )
                    }
                }

                MessageInput(
                    message = messageText,
                    onMessageChange = { messageText = it },
                    onSendClick = {
                        if (messageText.isNotBlank()) {
                            chatViewModel.sendMessage(messageText)
                            messageText = ""
                        }
                    }
                )
            }
        }
    )
}

/**
 * Barra superior específica do Chat.
 *
 * Exibe o avatar, nome do contacto/sala e estado (Online).
 *
 * @param contactName O nome a exibir no título.
 */
@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun ChatTopBar(contactName: String) {
    TopAppBar(
        title = {
            Row(verticalAlignment = Alignment.CenterVertically) {
                Surface(
                    shape = CircleShape,
                    color = MaterialTheme.colorScheme.secondary,
                    modifier = Modifier.size(20.dp)
                ) {
                    Box(contentAlignment = Alignment.Center) {
                        val initial = contactName.firstOrNull()?.toString()?.uppercase() ?: "?"
                        Text(
                            text = initial,
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
                        text = "Online",
                        fontSize = 12.sp,
                        color = MaterialTheme.colorScheme.onSurfaceVariant
                    )
                }
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
 * Componente visual que representa um balão de mensagem.
 *
 * Ajusta automaticamente o alinhamento, cor e forma (cantos arredondados) com base
 * em quem enviou a mensagem (Eu vs Outro).
 *
 * @param message O DTO contendo o texto da mensagem.
 * @param isSentByMe Booleano que indica se a mensagem pertence ao utilizador atual.
 */
@Composable
fun MessageBubble(message: MessageDto, isSentByMe: Boolean) {
    val alignment = if (isSentByMe) Alignment.CenterEnd else Alignment.CenterStart
    val bubbleColor =
        if (isSentByMe) MaterialTheme.colorScheme.primaryContainer else MaterialTheme.colorScheme.surfaceVariant
    val textColor =
        if (isSentByMe) MaterialTheme.colorScheme.onPrimaryContainer else MaterialTheme.colorScheme.onSurfaceVariant
    val bubbleShape = if (isSentByMe) {
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
 * Componente de entrada de texto para novas mensagens.
 *
 * Inclui um campo de texto arredondado e um botão de envio que só fica ativo se houver texto.
 *
 * @param message O texto atual no campo.
 * @param onMessageChange Callback para atualizar o estado do texto.
 * @param onSendClick Callback disparado ao carregar no botão de envio.
 */
@Composable
fun MessageInput(
    message: String,
    onMessageChange: (String) -> Unit,
    onSendClick: () -> Unit
) {
    Surface(
        tonalElevation = 2.dp,
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
                modifier = Modifier
                    .weight(1f)
                    .testTag(stringResource(R.string.tag_field_message)),
                shape = RoundedCornerShape(24.dp)
            )

            Spacer(modifier = Modifier.width(8.dp))

            IconButton(
                onClick = onSendClick,
                enabled = message.isNotBlank(),
                modifier = Modifier
                    .size(48.dp)
                    .clip(CircleShape)
                    .testTag(stringResource(R.string.tag_button_send_message))
            ) {
                Icon(
                    Icons.AutoMirrored.Filled.Send,
                    contentDescription = "Enviar mensagem",
                    tint = MaterialTheme.colorScheme.primary
                )
            }
        }
    }
}

/*
@Preview(name = "Chat Screen - English", locale = "en", showBackground = true)
@Preview(name = "Chat Screen - Portuguese", locale = "pt", showBackground = true)
@Composable
fun ChatScreenPreview() {
    // Mock de dados para visualizar o layout sem ViewModel
    val mockMessages = listOf(
        MessageDto(text = "Olá! Tudo bem?", senderId = "other"),
        MessageDto(text = "Tudo ótimo! E contigo?", senderId = "me"),
        MessageDto(text = "Vamos treinar amanhã?", senderId = "other"),
        MessageDto(text = "Claro, às 19h no campo principal.", senderId = "me")
    )

    MaterialTheme {
        Scaffold(
            topBar = { ChatTopBar(contactName = "Treinador João", onBackClick = {}) }
        ) { padding ->
            Column(
                modifier = Modifier
                    .fillMaxSize()
                    .padding(padding)
            ) {
                LazyColumn(
                    modifier = Modifier
                        .weight(1f)
                        .padding(horizontal = 16.dp)
                ) {
                    items(mockMessages) { msg ->
                        MessageBubble(
                            message = msg,
                            isSentByMe = (msg.senderId == "me")
                        )
                    }
                }
                MessageInput(message = "", onMessageChange = {}, onSendClick = {})
            }
        }
    }
}
*/