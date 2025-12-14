package com.example.amfootball.ui.components.camera

import android.content.Context
import android.net.Uri
import android.util.Log
import androidx.camera.core.CameraSelector
import androidx.camera.core.ImageCapture
import androidx.camera.core.ImageCaptureException
import androidx.camera.view.LifecycleCameraController
import androidx.camera.view.PreviewView
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Cameraswitch
import androidx.compose.material.icons.filled.Lens
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.runtime.Composable
import androidx.compose.runtime.remember
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.platform.LocalContext
import androidx.lifecycle.compose.LocalLifecycleOwner
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.unit.dp
import androidx.compose.ui.viewinterop.AndroidView
import androidx.core.content.ContextCompat
import androidx.core.content.FileProvider
import androidx.lifecycle.LifecycleOwner
import com.example.amfootball.R
import java.io.File
import java.text.SimpleDateFormat
import java.util.Date
import java.util.Locale

/**
 * Ecrã principal de captura de imagem utilizando a biblioteca CameraX.
 *
 * Este Composable orquestra a pré-visualização da câmara e os controlos de interface,
 * gerindo também o ciclo de vida do controlador da câmara.
 *
 * @param onImageCaptured Callback invocado com o [Uri] da imagem salva com sucesso.
 * @param onError Callback invocado com [ImageCaptureException] em caso de falha na captura.
 */
@Composable
fun CameraCaptureScreen(
    onImageCaptured: (Uri) -> Unit,
    onError: (ImageCaptureException) -> Unit
) {
    val context = LocalContext.current
    val lifecycleOwner = LocalLifecycleOwner.current

    // Controlador da Câmara que gere o ciclo de vida automaticamente
    val cameraController = remember {
        LifecycleCameraController(context).apply {
            setEnabledUseCases(
                LifecycleCameraController.IMAGE_CAPTURE or
                        LifecycleCameraController.VIDEO_CAPTURE
            )
        }
    }

    Box(modifier = Modifier.fillMaxSize()) {
        CameraPreview(
            cameraController = cameraController,
            lifecycleOwner = lifecycleOwner,
            modifier = Modifier.fillMaxSize()
        )

        CameraControls(
            onCaptureClick = {
                takePhoto(context, cameraController, onImageCaptured, onError)
            },
            onSwitchCameraClick = {
                toggleCamera(cameraController)
            },
            modifier = Modifier.fillMaxSize()
        )
    }
}

/**
 * Componente responsável por renderizar a pré-visualização da câmara.
 *
 * Utiliza o [AndroidView] para integrar o [PreviewView] do sistema de vistas clássico
 * no Jetpack Compose.
 *
 * @param cameraController O controlador do ciclo de vida da câmara.
 * @param lifecycleOwner O proprietário do ciclo de vida atual (geralmente a Activity ou Fragment).
 * @param modifier Modificador de layout a aplicar ao componente.
 */
@Composable
fun CameraPreview(
    cameraController: LifecycleCameraController,
    lifecycleOwner: LifecycleOwner,
    modifier: Modifier = Modifier
) {
    AndroidView(
        factory = { ctx ->
            PreviewView(ctx).apply {
                this.controller = cameraController
                cameraController.bindToLifecycle(lifecycleOwner)
            }
        },
        modifier = modifier
    )
}

/**
 * Camada de controlos da câmara sobreposta à pré-visualização.
 *
 * Inclui o botão de captura e o botão para alternar entre câmaras.
 *
 * @param onCaptureClick Ação a executar quando o botão de captura é pressionado.
 * @param onSwitchCameraClick Ação a executar quando o botão de troca de câmara é pressionado.
 * @param modifier Modificador de layout.
 */
@Composable
fun CameraControls(
    onCaptureClick: () -> Unit,
    onSwitchCameraClick: () -> Unit,
    modifier: Modifier = Modifier
) {
    Box(modifier = modifier) {
        // Botão de Trocar Câmara
        IconButton(
            onClick = onSwitchCameraClick,
            modifier = Modifier
                .align(Alignment.TopEnd)
                .padding(16.dp)
        ) {
            Icon(
                imageVector = Icons.Default.Cameraswitch,
                contentDescription = stringResource(id = R.string.change_camera),
                tint = Color.White,
                modifier = Modifier.size(32.dp)
            )
        }

        // Botão de Disparo
        IconButton(
            onClick = onCaptureClick,
            modifier = Modifier
                .align(Alignment.BottomCenter)
                .padding(bottom = 32.dp)
                .size(80.dp)
        ) {
            Icon(
                imageVector = Icons.Default.Lens,
                contentDescription = stringResource(id = R.string.take_pitcture),
                tint = Color.White,
                modifier = Modifier.fillMaxSize()
            )
        }
    }
}

/**
 * Alterna a lente da câmara entre frontal e traseira.
 *
 * Verifica a seleção atual e inverte-a.
 *
 * @param cameraController O controlador da câmara a ser atualizado.
 */
private fun toggleCamera(cameraController: LifecycleCameraController) {
    cameraController.cameraSelector =
        if (cameraController.cameraSelector == CameraSelector.DEFAULT_BACK_CAMERA) {
            CameraSelector.DEFAULT_FRONT_CAMERA
        } else {
            CameraSelector.DEFAULT_BACK_CAMERA
        }
}

/**
 * Lógica auxiliar para capturar a foto e salvar num ficheiro temporário.
 *
 * Cria um ficheiro local, configura o CameraX para escrever diretamente nele
 * e, após o sucesso, gera um URI seguro via FileProvider.
 *
 * @param context O contexto necessário para criar ficheiros e aceder a executores.
 * @param controller O controlador da câmara.
 * @param onPhotoTaken Callback de sucesso com o URI.
 * @param onError Callback de erro.
 */
private fun takePhoto(
    context: Context,
    controller: LifecycleCameraController,
    onPhotoTaken: (Uri) -> Unit,
    onError: (ImageCaptureException) -> Unit
) {
    val timeStamp = SimpleDateFormat("yyyyMMdd_HHmmss", Locale.getDefault()).format(Date())

    val photoFile = File(
        context.externalCacheDir ?: context.cacheDir,
        "JPEG_${timeStamp}.jpg"
    )

    val outputOptions = ImageCapture.OutputFileOptions.Builder(photoFile).build()

    controller.takePicture(
        outputOptions,
        ContextCompat.getMainExecutor(context),
        object : ImageCapture.OnImageSavedCallback {
            override fun onImageSaved(outputFileResults: ImageCapture.OutputFileResults) {
                val savedUri = FileProvider.getUriForFile(
                    context,
                    "${context.packageName}.provider",
                    photoFile
                )

                Log.d("CameraX", "Sucesso: $savedUri")
                onPhotoTaken(savedUri)
            }

            override fun onError(exception: ImageCaptureException) {
                Log.e("CameraX", "Erro ao capturar imagem: ${exception.message}", exception)
                onError(exception)
            }
        }
    )
}