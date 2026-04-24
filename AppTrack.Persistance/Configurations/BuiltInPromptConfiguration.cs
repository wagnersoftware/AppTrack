using AppTrack.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppTrack.Persistance.Configurations;

public class BuiltInPromptConfiguration : IEntityTypeConfiguration<BuiltInPrompt>
{
    public void Configure(EntityTypeBuilder<BuiltInPrompt> builder)
    {
        builder.ToTable("BuiltInPrompts");

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.PromptTemplate)
            .IsRequired();

        builder.HasIndex(x => x.Name)
            .IsUnique();

        builder.HasData(
            Seed(1, "builtIn_GenerateApplicationText",
                "Write a concise, authentic application email for the following freelance position.\n\nRules:\n- Sound professional but genuine — avoid clichés and generic phrases like \"I am the perfect fit\", \"highly motivated\", \"results-oriented\", \"passionate about\", or similar hollow filler\n- Connect my profile directly to the specific requirements in the job description — do NOT just list my skills, but explain HOW they are relevant to this role\n- Reference specific aspects of the job posting (required technologies, project type, challenges mentioned) and show how my experience addresses them\n- Keep it brief: 3–4 short paragraphs at most\n- Only use skills and experience explicitly listed in my CV and skills — do not invent skills\n- Begin with a strong, specific opening sentence that references this position concretely — not a generic \"I am applying for...\"\n\nMy profile:\nMy CV: {{builtIn_CvText}}\nMy Name: {{builtIn_FirstName}} {{builtIn_LastName}}\nMy Hourly Rate: {{builtIn_HourlyRate}}\nMy Daily Rate: {{builtIn_DailyRate}}\nMy Skills: {{builtIn_Skills}}\nMy Work Mode: {{builtIn_WorkMode}}\nAvailable From: {{builtIn_AvailableFrom}}\n\nJob posting:\nJob Description: {{JobDescription}}\nContact Person: {{ContactPerson}}\nPosition: {{Position}}"),
            Seed(2, "builtIn_JobMatchAnalysis",
                "Analyze how well my profile matches the following job posting. Be honest and direct — do NOT inflate the match score or soften weaknesses. If I am missing required skills, say so explicitly.\n\nYour response must cover:\n1. Match score: A percentage (0–100) with a one-sentence justification\n2. Skills I have that match: Reference my CV and skills list specifically\n3. Skills or experience I am missing: Be specific — what does the job require that I lack?\n4. Work mode / location fit: Is {{Location}} compatible with my preferred work mode ({{builtIn_WorkMode}})?\n5. Verdict: Should I apply? What are the main risks or gaps I would need to address?\n\nMy profile:\nCV: {{builtIn_CvText}}\nSkills: {{builtIn_Skills}}\nWork Mode Preference: {{builtIn_WorkMode}}\n\nJob posting:\nPosition: {{Position}}\nLocation: {{Location}}\nDuration: {{DurationInMonths}} months\nStart Date: {{StartDate}}\nJob Description: {{JobDescription}}"),
            Seed(3, "builtIn_InterviewPrep",
                "Prepare me for an interview for the {{Position}} role. Base all questions strictly on the job description — do not generate generic filler questions.\n\nProvide:\n1. 5–7 role-specific or technical questions the interviewer is likely to ask based on the actual requirements\n2. 2–3 behavioral questions relevant to this type of role\n3. For each question: a suggested answer approach based on my CV and skills. If my experience in a relevant area is weak, say so explicitly and suggest how to address it honestly rather than inventing strengths\n4. 2 questions I should ask the interviewer to evaluate whether this project is genuinely worth taking\n\nMy profile:\nMy Name: {{builtIn_FirstName}} {{builtIn_LastName}}\nMy CV: {{builtIn_CvText}}\nMy Skills: {{builtIn_Skills}}\n\nJob posting:\nPosition: {{Position}}\nLocation: {{Location}}\nDuration: {{DurationInMonths}} months\nStart Date: {{StartDate}}\nJob Description: {{JobDescription}}"),
            Seed(4, "builtIn_RateAnalysis",
                "Based on the job description and my CV, analyze what daily and hourly rate I can realistically charge for this role.\n\nYour response must include:\n1. Recommended rate range: A realistic daily rate range (minimum / target / optimal) I can ask for given my qualifications and the role requirements\n2. Justification: Why this range — reference specific skills from my CV that match or strengthen my position, and be honest about any gaps that would weaken it\n3. Minimum rate: The lowest daily rate I should accept for this role without underselling myself — explain why going below this would not be appropriate\n4. Rate risk factors: Are there aspects of my profile (e.g. missing skills, overqualification, location/remote constraints) that could affect what I can realistically negotiate?\n\nBe honest and direct. If my qualifications do not fully match the role, reflect that in a lower range rather than inflating the recommendation.\n\nMy profile:\nMy CV: {{builtIn_CvText}}\nMy Skills: {{builtIn_Skills}}\nMy Current Daily Rate: {{builtIn_DailyRate}}\nMy Current Hourly Rate: {{builtIn_HourlyRate}}\nWork Mode: {{builtIn_WorkMode}}\n\nJob posting:\nPosition: {{Position}}\nLocation: {{Location}}\nDuration: {{DurationInMonths}} months\nStart Date: {{StartDate}}\nJob Description: {{JobDescription}}")
        );
    }

    private static BuiltInPrompt Seed(int id, string name, string promptTemplate)
    {
        var p = BuiltInPrompt.Create(name, promptTemplate);
        p.Id = id;
        return p;
    }
}
