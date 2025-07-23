import puppeteer from 'puppeteer';
import odbc from 'odbc';
import CyrillicToTranslit from 'cyrillic-to-translit-js';

const connectionString = 'Driver={Microsoft Access Driver (*.mdb, *.accdb)};DBQ=C:\\VScodeParser\\TDAPZParser\\TDPAZ_db.accdb;';
const TestParserAmount = 200;

(async () => {
  const connection = await odbc.connect(connectionString);

  const browser = await puppeteer.launch({
    headless: true,
    executablePath: 'C:\\Program Files (x86)\\Microsoft\\Edge\\Application\\msedge.exe',
    args: ['--start-maximized', '--no-default-browser-check', '--no-first-run']
  });

  //cd C:\VScodeParser\TDAPZparser
  //node index.js

  const link = 'https://xxx.xxxx//yyy.yyyy';
  const cyrillicToTranslit = new CyrillicToTranslit();
  const page = await browser.newPage();
  
  const [width, height] = await page.evaluate(() => [window.screen.width, window.screen.height]);
  await page.setViewport({ width, height });

  if (!page.url().includes('view.php?id=921278')) {
    try {
      await page.goto(link, {
        waitUntil: 'load',
        timeout: 20000
      });
    } catch (error) {
      console.log('Goto page timeout, retrying...');
      await page.goto(link, {
        waitUntil: 'domcontentloaded',
        timeout: 20000
      });
    }
  }
  

  const isLoggedIn = await page.evaluate(() => {
    return !document.querySelector('img[src*="google.com"]');
  });

  if (!isLoggedIn) {
    console.log("\nAuthorization required.");
    await page.waitForSelector('img[src*="google.com"]');
    await page.evaluate(() => {
      const googleImg = document.querySelector('img[src*="google.com"]');
      if (googleImg) {
        const button = googleImg.closest('a, button');
        if (button) button.click();
      }
    });

    await page.waitForNavigation({ waitUntil: 'networkidle2' });
    await page.waitForSelector('input[type="email"]');
    await page.type('input[type="email"]', 'youremail@gmail.com', { delay: 10 });
    await page.keyboard.press('Enter');

    await page.waitForSelector('input[type="password"]', { visible: true });
    await page.type('input[type="password"]', 'youremailpassword', { delay: 10 });
    await page.keyboard.press('Enter');
    await page.waitForNavigation({ waitUntil: 'networkidle2' });
  } else {
    //console.log("User is already logged in.");
  }

  for (let i = 0; i < TestParserAmount; i++) {
    console.log(`\n=== Starting Test ${i + 1} ===\n`);

    try {
      await page.waitForSelector('button.btn.btn-primary', { visible: true, timeout: 5000 });
      await page.click('button.btn.btn-primary');
    } catch (error) {
      //console.log("Start attempt button not found. Possibly already on test page.");
    }

    try {
      await page.waitForSelector('input#id_submitbutton', { visible: true, timeout: 2500 });
      await page.click('input#id_submitbutton');
    } catch (error) {
      //console.log("Confirmation button not found. Continuing...");
    }

    try {
      await page.waitForSelector('input#mod_quiz-next-nav', { visible: true, timeout: 2500 });
      await page.click('input#mod_quiz-next-nav');
    } catch (error) {
      //console.log("Next button not found. Continuing...");
    }

    try {
      await page.waitForSelector('button.btn.btn-primary', { visible: true, timeout: 2500 });
      await page.click('button.btn.btn-primary');
    } catch (error) {
      console.log("Start test button not found. Continuing...");
    }

    try {
      await page.waitForSelector('button[data-action="save"]', { visible: true, timeout: 2500 });
      await page.click('button[data-action="save"]');
    } catch (error) {
      console.log("Save button not found. Continuing...");
    }

    await page.waitForNavigation({ waitUntil: 'networkidle2' });

    const questionsAndAnswers = await page.evaluate(() => {
      const questions = Array.from(document.querySelectorAll('.qtext'));
      const answers = Array.from(document.querySelectorAll('.rightanswer'));
      return questions.map((q, index) => ({
        question: q.innerText.trim(),
        answer: answers[index] ? answers[index].innerText.match(/: (\w+)/)?.[1] : null
      }));
    });

    for (const { question, answer } of questionsAndAnswers) {
      if (!question || !answer) {
        console.log('Question or Answer did not find');
        continue;
      }

      const QuestionTranslited = cyrillicToTranslit.transform(question).toLowerCase();
      //console.log(`\nOriginal Question: ${question}`);
      //console.log(`Transliterated Question: ${QuestionTranslited}`);
      //console.log(`Answer: ${answer}\n`);

      const escapedQuestion = question.replace(/'/g, "''");
      const escapedAnswer = answer.replace(/'/g, "''");

      const queryCheck = `SELECT COUNT(*) AS Count FROM QuestionsAndAnswers WHERE Question = '${escapedQuestion}'`;
      const queryInsert = `INSERT INTO QuestionsAndAnswers (Question, Answer) VALUES ('${escapedQuestion}', '${escapedAnswer}')`;

      try {
        const result = await connection.query(queryCheck);
        if (result[0].Count > 0) {
          console.log('Question already exists');
        } else {
          await connection.query(queryInsert);
          console.log('QUESTION & ANSWER added');
        }
      } catch (error) {
        console.error('Error with DB:', error.message);
      }
    }

    try {
      await page.waitForSelector('a.mod_quiz-next-nav, button#single_button680696d61037420, button.btn.btn-primary', { visible: true, timeout: 5000 });
    
      const buttonSelector = await page.evaluate(() => {
        if (document.querySelector('a.mod_quiz-next-nav')) return 'a.mod_quiz-next-nav';
        if (document.querySelector('button#single_button680696d61037420')) return 'button#single_button680696d61037420';
        if (document.querySelector('button.btn.btn-primary')) return 'button.btn.btn-primary';
        return null;
      });
    
      if (buttonSelector) {
        const currentUrl = page.url();
    
        await Promise.all([
          page.click(buttonSelector),
          page.waitForNavigation({ waitUntil: 'domcontentloaded', timeout: 10000 }).catch(() => null) // ⬅️ Навігація не обов'язкова
        ]);
    
        const newUrl = page.url();
        if (newUrl === currentUrl) {
          console.log("URL doesnt change? manual changing");
          await page.goto('https://vns.lpnu.ua/mod/quiz/view.php?id=921278', {
            waitUntil: 'domcontentloaded'
          });
        }
      } else {
        console.log("Buttom for cross did not find");
      }
    } catch (error) {
      console.log(`Navigation button error: ${error.message}`);
      await page.goto('https://vns.lpnu.ua/mod/quiz/view.php?id=921278', {
        waitUntil: 'domcontentloaded'
      });
    }
    

    console.log(`   Completed Test ${i + 1}`);
    await new Promise(resolve => setTimeout(resolve, 1000));
  }

  console.log("\n! All tests completed !\n");
  await connection.close();
  await browser.close();
})().catch(error => {
  console.error('Execution error:', error);
});