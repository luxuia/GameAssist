# GLM-4.6V API Integration Test Guide

## Overview
This document provides a comprehensive guide for testing the GLM-4.6V (智谱AI) API integration in the GameAssist application.

## Current Implementation Status

### ✅ Working Features

1. **Configuration Support**
   - Added `ApiProvider.ZhipuAI` enum value
   - Supports GLM-4V model (`glm-4.6v`)
   - Configurable endpoint: `https://open.bigmodel.cn/api/paas/v4/chat/completions`

2. **Request Format**
   - Uses correct GLM-4V API format
   - Image data embedded in `messages[0].content` array
   - Supports text and image_url content types
   - Uses `data:image/jpeg;base64,{base64Image}` format
   - Includes `detail: "high"` parameter for better image analysis

3. **Authentication**
   - Implements JWT token generation
   - Uses HMAC-SHA256 for signature
   - Token expires in 1 hour
   - API key format: `id.secret`

4. **Response Handling**
   - Properly deserializes GLM-4V response format
   - Extracts content from `choices[0].message.content`

### ✅ Verified Request Format

```json
{
  "model": "glm-4.6v",
  "messages": [
    {
      "role": "user",
      "content": [
        {
          "type": "text",
          "text": "Your prompt here"
        },
        {
          "type": "image_url",
          "image_url": {
            "url": "data:image/jpeg;base64,/9j/4AAQSkZJRgABAQEASABIAAD...",
            "detail": "high"
          }
        }
      ]
    }
  ],
  "max_tokens": 500,
  "temperature": 0.7,
  "stream": false
}
```

## Test Steps

### 1. Configure GLM-4.6V in Settings

1. Open GameAssist application
2. Go to Settings (Ctrl+S or File → Settings)
3. Select "Zhipu AI" as API Provider
4. Enter your API key in the format: `id.secret`
5. Verify endpoint is set to: `https://open.bigmodel.cn/api/paas/v4/chat/completions`
6. Select "glm-4.6v" as the model
7. Adjust other settings (tokens, temperature) as needed
8. Click "Save"

### 2. Test API Connection

1. In Settings window, click "Test API" button
2. The app will send a test image with a simple prompt
3. If successful, you should see "API test successful!" message
4. If failed, check:
   - API key format (must be `id.secret`)
   - Network connectivity
   - API key validity

### 3. Test with Dota 2 Screenshot

1. Start Dota 2 game
2. Ensure GameAssist is running
3. Wait for automatic analysis or click "Upload Screenshot"
4. Verify suggestions appear in both:
   - Main window "AI Suggestions" area
   - Transparent overlay (if enabled)

## Common Issues and Solutions

### 1. JWT Token Generation Errors

**Error**: "Invalid Zhipu AI API key format. Expected: id.secret"

**Solution**:
- Ensure API key is in correct format: `id.secret`
- Verify you're using a GLM-4V model API key, not other Zhipu API keys

### 2. API Authentication Errors

**Error**: "401 Unauthorized" or "Invalid API key"

**Solution**:
- Verify API key is valid and not expired
- Check that the key is for GLM-4V model access
- Ensure proper formatting (no extra spaces)

### 3. Image Format Issues

**Error**: Image not recognized by GLM-4V

**Solution**:
- Ensure images are in PNG/JPEG format
- Maximum image size limits (typically 4MB)
- Convert game screenshots to PNG before uploading

### 4. Response Parsing Issues

**Error**: Empty or null response content

**Solution**:
- Check API response for error messages
- Verify the prompt is clear and appropriate
- Try with different image content

## Configuration File Example

When using GLM-4.6V, your `config.json` should look like:

```json
{
  "Provider": "ZhipuAI",
  "OpenAiApiKey": "",
  "OpenAiEndpoint": "https://api.openai.com/v1/chat/completions",
  "ZhipuApiKey": "your-id.your-secret",
  "ZhipuEndpoint": "https://open.bigmodel.cn/api/paas/v4/chat/completions",
  "ModelName": "glm-4.6v",
  "MaxTokens": 500,
  "Temperature": 0.7,
  "IntervalSeconds": 60,
  "Dota2Only": true,
  "ShowPreview": true,
  "SelectedPrompt": 0,
  "CustomPrompt": "",
  "OverlayEnabled": true,
  "OverlayWidth": 600,
  "OverlayFontSize": 16,
  "OverlayAutoHideSeconds": 30,
  "AutoStart": false
}
```

## Debugging Tips

1. **Enable Logging**: Add logging statements around API calls to capture:
   - Generated JWT token
   - Request payload
   - Response status and content

2. **Check Network**: Use tools like Postman or curl to manually test the API:
   ```bash
   curl -X POST "https://open.bigmodel.cn/api/paas/v4/chat/completions" \
     -H "Authorization: Bearer YOUR_JWT_TOKEN" \
     -H "Content-Type: application/json" \
     -d '{
       "model": "glm-4.6v",
       "messages": [{
         "role": "user",
         "content": [{
           "type": "text",
           "text": "Test"
         }]
       }],
       "max_tokens": 100
     }'
   ```

3. **Monitor Usage**: Keep track of API usage through the Zhipu AI dashboard to avoid quota limits.

## Known Limitations

1. GLM-4V has different pricing compared to GPT-4V
2. Image processing limits may vary
3. Token counting works differently
4. Rate limits may be different from OpenAI

## Success Criteria

Successful integration is confirmed when:
1. ✅ Settings can save GLM-4.6V configuration
2. ✅ "Test API" button returns successful response
3. ✅ Game screenshots can be analyzed
4. ✅ Suggestions appear in both UI areas
5. ✅ Overlay window displays properly