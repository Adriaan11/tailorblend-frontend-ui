# Prompt Evaluation API Documentation

This document describes the backend API endpoints needed to support the Prompt Evaluation feature.

## Overview

The Prompt Evaluation feature allows users to test and improve system prompts by running them against multiple test cases and receiving scored feedback. This helps create better prompts for the blend assistant.

## Required Endpoints

### 1. Evaluate Prompt

**Endpoint:** `POST /api/prompt-evaluation/evaluate`

**Purpose:** Run a prompt against multiple test cases and return evaluation results with scores and feedback.

**Request Body:**
```json
{
  "prompt_text": "System prompt to evaluate...",
  "test_cases": [
    {
      "id": "uuid-here",
      "name": "Energy & Fatigue",
      "user_message": "I'm always tired, even after sleeping 8 hours. What can help?",
      "expected_characteristics": [
        "Ask about sleep quality",
        "Inquire about stress levels",
        "Suggest energy-supporting supplements"
      ],
      "avoid_characteristics": [
        "Immediate diagnosis",
        "Excessive medical jargon"
      ]
    }
  ],
  "model": "gpt-5-mini-2025-01-24",
  "evaluation_criteria": [
    "accuracy",
    "helpfulness",
    "safety",
    "tone",
    "completeness"
  ]
}
```

**Response Body:**
```json
{
  "overall_score": 85.5,
  "test_results": [
    {
      "test_case_id": "uuid-here",
      "test_case_name": "Energy & Fatigue",
      "response": "AI's actual response to the test case...",
      "score": 87.0,
      "feedback": "The response demonstrates good understanding of the user's concern...",
      "strengths": [
        "Empathetic and professional tone",
        "Asked relevant follow-up questions",
        "Provided actionable recommendations"
      ],
      "weaknesses": [
        "Could have inquired about medical conditions",
        "Didn't mention when to seek professional help"
      ],
      "suggestions": [
        "Add a safety check for underlying medical conditions",
        "Include guidance on when to consult a healthcare provider"
      ],
      "tokens": {
        "input_tokens": 250,
        "output_tokens": 180,
        "total_tokens": 430
      }
    }
  ],
  "summary": "Overall, the prompt performs well with an average score of 85.5. Strengths include...",
  "recommendations": [
    "Add more explicit safety guidelines",
    "Clarify when to recommend medical consultation",
    "Improve specificity in supplement recommendations"
  ],
  "total_tokens": {
    "input_tokens": 1500,
    "output_tokens": 1200,
    "total_tokens": 2700
  },
  "cost_zar": 0.45,
  "evaluation_time_seconds": 12.5
}
```

**Implementation Notes:**

The backend should:

1. **For each test case:**
   - Use the provided `prompt_text` as the system prompt
   - Send the `user_message` to the AI model
   - Collect the AI's response

2. **Evaluate the response:**
   - Use an LLM-as-judge approach to score the response
   - Check if it exhibits the `expected_characteristics`
   - Check if it avoids the `avoid_characteristics`
   - Score each response 0-100 based on the evaluation criteria

3. **Generate feedback:**
   - Identify specific strengths in the response
   - Identify specific weaknesses
   - Provide actionable suggestions for improvement

4. **Calculate metrics:**
   - Track token usage for cost calculation
   - Calculate overall score as average of all test case scores
   - Generate summary of results
   - Provide high-level recommendations

**Example Implementation Pattern:**

```python
@router.post("/api/prompt-evaluation/evaluate")
async def evaluate_prompt(request: PromptEvaluationRequest):
    results = []

    for test_case in request.test_cases:
        # Run the AI with the test prompt
        response = await run_ai_with_prompt(
            system_prompt=request.prompt_text,
            user_message=test_case.user_message,
            model=request.model
        )

        # Use LLM-as-judge to evaluate the response
        evaluation = await evaluate_response(
            response=response,
            expected_characteristics=test_case.expected_characteristics,
            avoid_characteristics=test_case.avoid_characteristics,
            criteria=request.evaluation_criteria
        )

        results.append({
            "test_case_id": test_case.id,
            "test_case_name": test_case.name,
            "response": response.content,
            "score": evaluation.score,
            "feedback": evaluation.feedback,
            "strengths": evaluation.strengths,
            "weaknesses": evaluation.weaknesses,
            "suggestions": evaluation.suggestions,
            "tokens": response.tokens
        })

    overall_score = sum(r["score"] for r in results) / len(results)

    # Generate summary and recommendations
    summary = await generate_summary(results)
    recommendations = await generate_recommendations(results)

    return {
        "overall_score": overall_score,
        "test_results": results,
        "summary": summary,
        "recommendations": recommendations,
        "total_tokens": calculate_total_tokens(results),
        "cost_zar": calculate_cost(results),
        "evaluation_time_seconds": elapsed_time
    }
```

### 2. Get Default Test Cases

**Endpoint:** `GET /api/prompt-evaluation/default-test-cases`

**Purpose:** Return a set of predefined test cases for blend assistant evaluation.

**Response Body:**
```json
[
  {
    "id": "uuid-1",
    "name": "Energy & Fatigue",
    "user_message": "I'm always tired, even after sleeping 8 hours. What can help?",
    "expected_characteristics": [
      "Ask about sleep quality",
      "Inquire about stress levels",
      "Suggest energy-supporting supplements",
      "Recommend consultation if severe"
    ],
    "avoid_characteristics": [
      "Immediate diagnosis",
      "Excessive medical jargon",
      "Pushy sales language"
    ]
  },
  {
    "id": "uuid-2",
    "name": "Medication Interaction Check",
    "user_message": "I'm taking blood pressure medication. Is it safe to take supplements?",
    "expected_characteristics": [
      "Request specific medication names",
      "Cautious and safety-focused",
      "Recommend consulting healthcare provider",
      "Flag potential interactions"
    ],
    "avoid_characteristics": [
      "Dismissing medication concerns",
      "Providing medical advice",
      "Proceeding without proper safety checks"
    ]
  }
]
```

**Implementation Notes:**

- Store common test scenarios in a configuration file or database
- Include diverse scenarios: safety checks, common symptoms, edge cases
- Update test cases based on common user patterns
- Consider categorizing by: safety, efficacy, tone, completeness

## Frontend Behavior (Current Implementation)

The frontend currently includes **fallback behavior** for development:

1. **If backend endpoint exists:** Uses the real evaluation endpoint
2. **If backend returns 404:** Displays mock evaluation data with a notice
3. **Mock data includes:**
   - Simulated scores (60-95 range)
   - Generic feedback messages
   - Notice that backend is not yet implemented

This allows frontend development and testing to proceed independently of backend implementation.

## Evaluation Strategy Recommendations

### LLM-as-Judge Pattern

Use a separate LLM call to evaluate the quality of responses:

```python
evaluation_prompt = f"""
You are evaluating an AI assistant's response for quality and appropriateness.

Original user question: {test_case.user_message}
AI's response: {ai_response}

Expected characteristics:
{chr(10).join('- ' + c for c in test_case.expected_characteristics)}

Should avoid:
{chr(10).join('- ' + c for c in test_case.avoid_characteristics)}

Evaluate on these criteria:
{chr(10).join('- ' + c for c in evaluation_criteria)}

Provide:
1. Score (0-100)
2. Brief feedback on overall quality
3. List of strengths (2-4 items)
4. List of weaknesses (1-3 items)
5. Actionable suggestions for improvement (2-4 items)

Format as JSON.
"""
```

### Scoring Guidelines

- **90-100:** Exceptional - demonstrates all expected characteristics, avoids all problems
- **80-89:** Strong - hits most expected characteristics, minor improvements needed
- **70-79:** Good - generally appropriate but missing some key elements
- **60-69:** Adequate - basic requirements met but significant gaps
- **50-59:** Weak - misses several important characteristics
- **Below 50:** Poor - fundamental issues with response quality or safety

### Cost Optimization

- Use cheaper models (e.g., GPT-4o mini) for the evaluation LLM-as-judge
- Cache evaluation results for identical test case + prompt combinations
- Consider batching multiple evaluations in a single call
- Provide token estimates before running full evaluation

## Security Considerations

- Validate prompt length (prevent abuse)
- Rate limit evaluation requests
- Log all evaluations for audit trail
- Sanitize user-provided test cases
- Ensure prompts don't contain sensitive information

## Future Enhancements

1. **Batch Evaluation:** Support evaluating multiple prompt versions simultaneously
2. **A/B Testing:** Compare two prompts side-by-side
3. **Historical Tracking:** Store evaluation results over time
4. **Automated Regression Testing:** Run evaluations on prompt changes automatically
5. **Custom Evaluation Criteria:** Allow users to define their own scoring criteria
6. **Export Results:** Download evaluation reports as PDF or CSV

## Error Handling

Return appropriate HTTP status codes:

- `200 OK`: Evaluation successful
- `400 Bad Request`: Invalid request (missing fields, invalid model, etc.)
- `429 Too Many Requests`: Rate limit exceeded
- `500 Internal Server Error`: Unexpected error during evaluation

Example error response:
```json
{
  "error": "Invalid request",
  "message": "Prompt text is required",
  "code": "MISSING_PROMPT_TEXT"
}
```

## Testing the Implementation

Use these test cases to verify the backend:

```bash
# 1. Test evaluation endpoint
curl -X POST http://localhost:5000/api/prompt-evaluation/evaluate \
  -H "Content-Type: application/json" \
  -d @test_evaluation_request.json

# 2. Test default test cases
curl http://localhost:5000/api/prompt-evaluation/default-test-cases
```

## Questions?

Contact the frontend team for clarification on the integration requirements.
